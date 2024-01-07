using System.Text;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Logging;
using static CounterStrikeSharp.API.Core.Listeners;

namespace SklepCSManager;


public partial class SklepcsManagerPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "SklepCS Manager Plugin";
    public override string ModuleAuthor => "Hacker";
    public override string ModuleVersion => "1.2.0";
    public override string ModuleDescription => "https://github.com/CS-GEJMERZY/SklepCS-Manager";

    public PluginConfig Config { get; set; }

    internal DatabaseManager? DatabaseManager;
    internal PermissionManager? PermissionManager;
    internal SklepcsWebManager? WebManager;

    internal Dictionary<CCSPlayerController, Player> PlayerCache = new();

    public string PluginChatPrefix { get; set; } = " DefaultPrefix";

    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;

        DatabaseManager = new DatabaseManager(Config.Settings.Database);
        PermissionManager = new PermissionManager(Config.PermissionGroups);
        WebManager = new SklepcsWebManager(Config.Sklepcs.ServerTag, Config.Sklepcs.ApiKey);


        PluginChatPrefix = Config.Settings.Prefix;
        PreparePluginPrefix();
    }

    public override void Load(bool hotReload)
    {
        RegisterListener<OnClientDisconnect>(OnClientDisconnect);
        Console.WriteLine("SklepCS Plugin loaded. ");


        if (Config.Sklepcs.WebFeaturesEnabled)
        {
            Task.Run(async () =>
            {
                bool webServices = await WebManager!.LoadWebServices();


                bool settings = await WebManager!.LoadWebSettings();

                Server.NextFrame(() =>
                {
                    if (webServices)
                    {
                        Server.PrintToConsole("Web services loaded.");
                    }
                    else
                    {
                        if (Config.Settings.IsLoggingLevelEnabled(LoggingLevelData.WebApiErrors))
                        {
                            Logger.LogError($"Failed to load web services. DEBUG: {WebManager.GetDebugData()}");
                        }
                    }

                    if (settings)
                    {
                        Server.PrintToConsole("Web settings loaded.");
                    }
                    else
                    {
                        if (Config.Settings.IsLoggingLevelEnabled(LoggingLevelData.WebApiErrors))
                        {
                            Logger.LogError($"Failed to load web settings. DEBUG: {WebManager.GetDebugData()}");
                        }
                    }
                });


            });
        }


        if (hotReload)
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (player != null && player.IsValid && !player.IsBot && player.AuthorizedSteamID != null)
                {
                    PlayerCache.Add(player, new Player());
                    var steamId2 = player.AuthorizedSteamID.SteamId2;
                    var steamId64 = player.AuthorizedSteamID.SteamId64;
                    Task.Run(async () =>
                    {
                        await PlayerCache[player].LoadDatabaseData(steamId2, Config.Sklepcs.ServerTag, DatabaseManager!);
                        await PlayerCache[player].LoadSklepcsData(steamId64, WebManager!);

                        Server.NextFrame(() =>
                        {
                            PlayerCache[player].AssignPermissions(player, PermissionManager!);
                        });
                    });
                }
            }
        }
    }

    [RequiresPermissions("@css/root")]
    [ConsoleCommand("css_sklepdebug", "Sklep debug")]
    public void OnDebugCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null || !PlayerCache.ContainsKey(player))
        {
            player!.PrintToChat($"{PluginChatPrefix}{Localizer["player.invalid"]}");
            return;
        }

        player!.PrintToChat("Listing flags loaded from database: ");
        foreach (var data in PlayerCache[player].ConnectionData)
        {
            player!.PrintToChat($"{data.Flags}");
        }

        player!.PrintToChat("Listing permissions loaded from config: ");
        foreach (var data in Config.PermissionGroups)
        {
            var stringBuilder = new StringBuilder();
            foreach (var perm in data.Permissions)
            {
                stringBuilder.Append(perm);
                stringBuilder.Append(", ");
            }
            string perms = stringBuilder.ToString().TrimEnd(',', ' ');
            commandInfo.ReplyToCommand($"{data.RequiredFlags} | {perms}");
        }

        var fetchedPermissions = PermissionManager!.FetchPermissions(PlayerCache[player].ConnectionData);

        player!.PrintToChat("Listing permissions that should be added: ");
        foreach (var permission in fetchedPermissions)
        {
            player!.PrintToChat($"{permission} | {(AdminManager.PlayerHasPermissions(player, permission) ? "Has" : "Doesn't have")}");
        }

        player!.PrintToChat("Listing added permissions: ");
        foreach (var permission in PlayerCache[player].AddedPermissions)
        {
            player!.PrintToChat($"{permission} | {(AdminManager.PlayerHasPermissions(player, permission) ? "Has" : "Doesn't have")}");
        }
    }
}

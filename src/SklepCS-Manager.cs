using System.Text;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Logging;

namespace SklepCSManager;


public partial class SklepcsManagerPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "SklepCS Manager Plugin";
    public override string ModuleAuthor => "Hacker";
    public override string ModuleVersion => "1.0.0";
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
        if(Config.Sklepcs.WebFeaturesEnabled)
        {
            Task.Run(async () =>
            {
                bool webServices = await WebManager!.LoadWebServices();

                await Task.Delay(500);
                bool settings = await WebManager!.LoadWebSettings();

                if (!webServices)
                {
                    Server.NextFrame(() =>
                    {
                        Logger.LogError($"Failed to load web services. DEBUG: {WebManager.GetDebugData()}");
                    });
                }

                if (!settings)
                {
                    Server.NextFrame(() =>
                    {
                        Logger.LogError($"Failed to load web settings. DEBUG: {WebManager.GetDebugData()}");

                    });
                }
            });
        }

        Console.WriteLine("SklepCS Plugin loaded. ");

        RegisterListener<Listeners.OnClientDisconnect>((slot) => { OnClientDisconnect(slot); });

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
            commandInfo.ReplyToCommand("Invalid player.");
            return;
        }

        commandInfo.ReplyToCommand("Listing flags loaded from database: ");
        foreach (var data in PlayerCache[player].ConnectionData)
        {
            commandInfo.ReplyToCommand($"{data.Flags}");
        }

        commandInfo.ReplyToCommand("Listing permissions loaded from config: ");
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

        commandInfo.ReplyToCommand("Listing permissions that should be added: ");
        foreach (var permission in fetchedPermissions)
        {
            commandInfo.ReplyToCommand($"{permission} | {(AdminManager.PlayerHasPermissions(player, permission) ? "Has" : "Doesn't have")}");
        }

        commandInfo.ReplyToCommand("Listing added permissions: ");
        foreach (var permission in PlayerCache[player].AddedPermissions)
        {
            commandInfo.ReplyToCommand($"{permission} | {(AdminManager.PlayerHasPermissions(player, permission) ? "Has" : "Doesn't have")}");
        }
    }
}

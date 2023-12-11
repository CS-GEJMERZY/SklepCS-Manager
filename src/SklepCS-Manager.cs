using System.Text;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace SklepCSManager;


public partial class SklepcsManagerPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "SklepCS Manager Plugin";
    public override string ModuleAuthor => "Hacker";
    public override string ModuleVersion => "0.0.1";

    public PluginConfig Config { get; set; }

    internal SklepcsDatabaseManager? DatabaseManager;
    internal SklepcsPermissionManager? PermissionManager;
    internal SklepcsWebManager? WebManager;


    internal Dictionary<CCSPlayerController, Player> PlayerCache = new();

    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;

        DatabaseManager = new SklepcsDatabaseManager(Config.Settings.Database);
        PermissionManager = new SklepcsPermissionManager(Config.PermissionGroups);
        WebManager = new SklepcsWebManager(Config.Sklepcs.ServerTag, Config.Sklepcs.ApiKey);
    }

    public override void Load(bool hotReload)
    {
        Task.Run(async () =>
        {
            await WebManager!.LoadWebServices();
            await WebManager!.LoadWebSettings();
        });
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
                    Task.Run(async () =>
                    {
                        await PlayerCache[player].LoadDatabaseData(steamId2, Config.Sklepcs.ServerTag, DatabaseManager!);

                        Server.NextFrame(() =>
                        {
                            PlayerCache[player].LoadPermissions(player, PermissionManager!);
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

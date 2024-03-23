using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using static CounterStrikeSharp.API.Core.Listeners;

namespace SklepCSManager;


public partial class SklepcsManagerPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "SklepCS Manager Plugin";
    public override string ModuleAuthor => "Hacker";
    public override string ModuleVersion => "1.2.1(beta)";
    public override string ModuleDescription => "https://github.com/CS-GEJMERZY/SklepCS-Manager";

    public required PluginConfig Config { get; set; }

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
                try
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
                }
                catch (Exception ex)
                {
                    Server.NextFrame(() => throw ex);
                }

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
                        try
                        {
                            await PlayerCache[player].LoadDatabaseData(steamId2, Config.Sklepcs.ServerTag, DatabaseManager!);
                            await PlayerCache[player].LoadSklepcsData(steamId64, WebManager!);
                        }
                        catch (Exception ex)
                        {
                            Server.NextFrame(() => throw ex);
                        }

                        Server.NextFrame(() =>
                        {
                            PlayerCache[player].AssignPermissions(player, PermissionManager!);
                        });
                    });
                }


            }
        }
    }
}




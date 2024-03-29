﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using static CounterStrikeSharp.API.Core.Listeners;

namespace Plugin;

public partial class SklepcsManagerPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "SklepCS Manager Plugin";
    public override string ModuleAuthor => "Hacker";
    public override string ModuleVersion => "1.2.2";
    public override string ModuleDescription => "https://github.com/CS-GEJMERZY/SklepCS-Manager";

    public required PluginConfig Config { get; set; }

    internal Managers.DatabaseManager? DatabaseManager;
    internal Managers.PermissionManager? PermissionManager;
    internal Managers.SklepcsWebManager? WebManager;

    internal Dictionary<CCSPlayerController, Managers.Player> PlayerCache = new();

    public string PluginChatPrefix { get; set; } = " DefaultPrefix";

    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;

        DatabaseManager = new Managers.DatabaseManager(Config.Settings.Database);
        PermissionManager = new Managers.PermissionManager(Config.PermissionGroups);
        WebManager = new Managers.SklepcsWebManager(Config.Sklepcs.ServerTag, Config.Sklepcs.ApiKey);


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
                        if (Config.Settings.IsLoggingLevelEnabled(Models.LoggingLevelData.WebApiErrors))
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
                        if (Config.Settings.IsLoggingLevelEnabled(Models.LoggingLevelData.WebApiErrors))
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
                    PlayerCache.Add(player, new Managers.Player());
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


}

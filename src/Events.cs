using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;

namespace SklepCSManager;


public partial class SklepcsManagerPlugin
{
    private void OnClientAuthorized(int playerSlot, SteamID steamID)
    {
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);

        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV || player.AuthorizedSteamID == null)
        {
            return;
        }

        if (!PlayerCache.ContainsKey(player)) { PlayerCache.Add(player, new Player()); }
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

    private void OnClientDisconnect(int playerSlot)
    {
        CCSPlayerController player = Utilities.GetPlayerFromSlot(playerSlot);

        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV)
        {
            return;
        }

        if (PlayerCache.ContainsKey(player))
        {
            PlayerCache.Remove(player);
        }
    }
}


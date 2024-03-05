using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;

namespace Plugin;

public partial class SklepcsManagerPlugin
{
    [GameEventHandler]
    public HookResult EventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV || player.AuthorizedSteamID == null)
        {
            return HookResult.Continue;
        }

        if (!PlayerCache.ContainsKey(player)) { PlayerCache.Add(player, new Managers.Player()); }
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

        return HookResult.Continue;
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


using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;


namespace SklepCSManager;


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

        if (!PlayerCache.ContainsKey(player)) { PlayerCache.Add(player, new Player()); }
        PlayerCache[player].LoadDatabaseData(player, DatabaseManager, Config.Settings.ServerTag);
        PlayerCache[player].LoadPermissions(player, PermissionManager);

        return HookResult.Continue;
    }

    private void OnClientDisconnect(int playerSlot)
    {
        CCSPlayerController player = Utilities.GetPlayerFromSlot(playerSlot);

        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV)
        {
            return;
        }

        PlayerCache.Remove(player);
    }
}


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

        if (!PlayerCache.TryGetValue(player, out Player? playerData)) 
        { 
            playerData = new Player(); PlayerCache.Add(player, playerData); 
        }
        var steamId2 = player.AuthorizedSteamID.SteamId2;
        var steamId64 = player.AuthorizedSteamID.SteamId64;
        Task.Run(async () =>
        {
            await playerData.LoadDatabaseData(steamId2, Config.Sklepcs.ServerTag, DatabaseManager!);
            await playerData.LoadSklepcsData(steamId64, WebManager!);
            Server.NextFrame(() =>
            {
                playerData.AssignPermissions(player, PermissionManager!);
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

        PlayerCache.Remove(player);
    }
}


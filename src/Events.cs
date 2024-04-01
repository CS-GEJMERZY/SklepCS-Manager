using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Microsoft.Extensions.Logging;

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

        if (!PlayerCache.TryGetValue(player, out Managers.PlayerManager? playerData))
        {
            playerData = new Managers.PlayerManager(); PlayerCache.Add(player, playerData);
        }

        var steamId2 = player.AuthorizedSteamID.SteamId2;
        var steamId64 = player.AuthorizedSteamID.SteamId64;
        Task.Run(async () =>
        {
            try
            {
                await playerData.LoadDatabaseData(steamId2, Config.Sklepcs.ServerTag, DatabaseManager!);
                await playerData.LoadSklepcsData(steamId64, WebManager!);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return;
            }

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


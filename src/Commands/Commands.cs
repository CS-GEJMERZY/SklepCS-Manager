using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace Plugin
{
    public partial class SklepcsManagerPlugin
    {
        private void HandlePlayerCommand(CCSPlayerController? player, CommandInfo commandInfo, Action action)
        {
            if (!Managers.PlayerManager.IsValid(player))
            {
                player!.PrintToChat($"{PluginChatPrefix}{Localizer["player.invalid"]}");
                return;
            }

            if (!PlayerCache.ContainsKey(player!))
            {
                player!.PrintToChat($"{PluginChatPrefix}{Localizer["player.no_data"]}");
                return;
            }

            action.Invoke();
        }
    }

}





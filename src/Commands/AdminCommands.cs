using System.Text;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace Plugin
{
    public partial class SklepcsManagerPlugin
    {
        [RequiresPermissions("@css/root")]
        [ConsoleCommand("css_sklepdebug", "Sklep debug")]
        public void OnDebugCommand(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (player == null || !PlayerCache.ContainsKey(player))
            {
                player!.PrintToChat($"{PluginChatPrefix}{Localizer["player.invalid"]}");
                return;
            }

            player!.PrintToChat("Listing flags loaded from database: ");
            foreach (var data in PlayerCache[player].ConnectionData)
            {
                player!.PrintToChat($"{data.Flags}");
            }

            player!.PrintToChat("Listing permissions loaded from config: ");
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

            player!.PrintToChat("Listing permissions that should be added: ");
            foreach (var permission in fetchedPermissions)
            {
                player!.PrintToChat($"{permission} | {(AdminManager.PlayerHasPermissions(player, permission) ? "Has" : "Doesn't have")}");
            }

            player!.PrintToChat("Listing added permissions: ");
            foreach (var permission in PlayerCache[player].AddedPermissions)
            {
                player!.PrintToChat($"{permission} | {(AdminManager.PlayerHasPermissions(player, permission) ? "Has" : "Doesn't have")}");
            }
        }
    }
}
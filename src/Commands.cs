using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace SklepCSManager;

public partial class SklepcsManagerPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    [ConsoleCommand("css_shop", "Sklep serwera")]
    [ConsoleCommand("css_sklep", "Sklep serwera")]
    public void OnShopCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null) { return; }

        commandInfo.ReplyToCommand($"{Config.Settings.Prefix} {ChatColors.Blue}Usługi zakupisz w naszym sklepie WWW: {ChatColors.LightPurple}{Config.Sklepcs.WebsiteURL}");

        if (PlayerCache[player].ConnectionData.Count > 0)
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix} {ChatColors.Blue}Twoje aktywne usługi:");
            for (int i = 0; i < PlayerCache[player].ConnectionData.Count; i++)
            {
                DateTime dateTime = PlayerCache[player].ConnectionData[i].End;
                commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{ChatColors.Darkred}#{i + 1} {ChatColors.Yellow}Koniec: {dateTime.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
        }
    }
}


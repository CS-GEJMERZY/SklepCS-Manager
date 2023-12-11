
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace SklepCSManager;

public partial class SklepcsManagerPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    [ConsoleCommand("css_uslugi", "Shows players active services")]
    public void OnServicesCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!Player.IsValid(player) && !PlayerCache.ContainsKey(player!))
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix} {ChatColors.Red}Nie znaleziono twoich danych w bazie danych, spróbuj ponownie za chwilę.");
            return;
        }

        if (PlayerCache[player!].ConnectionData.Count > 0)
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix} {ChatColors.Blue}Twoje aktywne usługi:");
            for (int i = 0; i < PlayerCache[player!].ConnectionData.Count; i++)
            {
                DateTime dateTime = PlayerCache[player!].ConnectionData[i].End;
                commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{ChatColors.Darkred}#{i + 1} {ChatColors.Yellow}Koniec: {dateTime:yyyy-MM-dd HH:mm:ss}");
            }
        }
    }

    [ConsoleCommand("css_sklepsms", "Main shop command")]
    public void OnShopCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!Player.IsValid(player) && !PlayerCache.ContainsKey(player!))
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix} {ChatColors.Red}Nie znaleziono twoich danych w bazie danych, spróbuj ponownie za chwilę.");
            return;
        }

        if (!WebManager!.IsLoaded)
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix} {ChatColors.Red}Sklep jest obecnie niedostępny, spróbuj ponownie za chwilę.");
            return;
        }

        player!.PrintToChat($"{Config.Settings.Prefix} {ChatColors.Purple}Dostępne usługi({WebManager.Services.Count})");

        for (int index = 0; index < WebManager.Services.Count; index++)
        {
            var service = WebManager.Services[index];
            player!.PrintToChat($"{Config.Settings.Prefix} {ChatColors.Darkred}#{index + 1} {ChatColors.Yellow}{service.Name} {ChatColors.Darkred}({service.Count} {service.Unit}) {ChatColors.Yellow}za {service.PlanValue / 100} {WebManager.CurrencyName}");
        }

    }
}


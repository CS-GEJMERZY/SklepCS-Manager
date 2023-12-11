
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API

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

        player!.PrintToChat($"{Config.Settings.Prefix} {ChatColors.Purple}Aby kupić usługę wpisz {ChatColors.Yellow}!kupsms <numer usługi> <kodsms>");
        player!.PrintToChat($"{Config.Settings.Prefix} {ChatColors.Gold}Przykład: {ChatColors.Yellow}!kupsms 1 123456");
    }

    [ConsoleCommand("css_kupsms", "Buys service")]
    public void OnBuyCommand(CCSPlayerController? player, CommandInfo commandInfo)
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

        if(commandInfo.ArgCount < 2)
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix} {ChatColors.Red}Niepoprawna komenda, wpisz {ChatColors.Yellow}!sklepsms {ChatColors.Red}aby zobaczyć dostępne usługi.");
            return;
        }
        
        string planIDString = commandInfo.GetArg(1);

        if(!int.TryParse(planIDString, out int planID))
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix} {ChatColors.Red}Niepoprawna komenda, wpisz {ChatColors.Yellow}!sklepsms {ChatColors.Red}aby zobaczyć dostępne usługi.");
            return;
        }

        string smsCode = commandInfo.GetArg(2);


        var steamId64 = player!.AuthorizedSteamID!.SteamId64;
        ServiceSmsData data = WebManager!.GetService(planID);

        if(data == null)
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix} {ChatColors.Red}Niepoprawna komenda, wpisz {ChatColors.Yellow}!sklepsms {ChatColors.Red}aby zobaczyć dostępne usługi.");
            return;
        }

        string playerIP = player.IpAddress.Split(':')[0];
        string playerName = player.PlayerName;

        Task.Run( async () =>
        {

            bool success = await WebManager!.RegisterServiceBuy(steamId64, data.PlanCode, smsCode, playerIP, playerName);

            if (success)
            {
                Server.NextFrame(() =>
                {
                    player!.PrintToChat($"{Config.Settings.Prefix} {ChatColors.Green}Usługa została zakupiona.");
                });
            }
            else
            {
                Server.NextFrame(() =>
                {
                    player!.PrintToChat($"{Config.Settings.Prefix} {ChatColors.Red}Nie udało się zakupić usługi.");
                });
            }
        });

    }
}



using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using Microsoft.Extensions.Logging;

namespace SklepCSManager;


public partial class SklepcsManagerPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    [ConsoleCommand("css_uslugi", "Show players active services")]
    public void OnServicesCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!Player.IsValid(player))
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["player.invalid"]}");
            return;
        }

        if (!PlayerCache.ContainsKey(player!))
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["player.no_data"]}");
            Logger.LogWarning($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) tried to use command without data");
            return;
        }

        if (PlayerCache[player!].ConnectionData.Count > 0)
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["services.your_services"]}");
            for (int i = 0; i < PlayerCache[player!].ConnectionData.Count; i++)
            {
                DateTime dateTime = PlayerCache[player!].ConnectionData[i].End;
                string formatedTime = dateTime.ToString(Localizer["services.datetime"]);

                commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["services.entry", i + 1, formatedTime]}");
            }
        }
    }

    [ConsoleCommand("css_sklepsms", "")]
    public void OnShopCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!Player.IsValid(player))
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["player.invalid"]}");
            return;
        }

        if (!PlayerCache.ContainsKey(player!))
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["player.no_data"]}");
            Logger.LogWarning($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) tried to use command without data");
            return;
        }

        if (!WebManager!.IsAvailable)
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["services.no_services"]}");
            return;
        }


        player!.PrintToChat($"{Config.Settings.Prefix}{Localizer["services.available", WebManager.Services.Count]}");

        for (int i = 0; i < WebManager.Services.Count; i++)
        {
            var service = WebManager.Services[i];
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["services.shopservice_entry",
               i + 1, service.Name, service.Count, service.Unit, service.PlanValue / (float)100, WebManager.CurrencyName]}");
        }

        commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["services.how_to_buy"]}");
        commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["services.how_to_buy_example"]}");
    }


    [ConsoleCommand("css_kupusluge", "")]
    public void OnShopBuyCommmand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!Player.IsValid(player))
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["player.invalid"]}");
            return;
        }

        if (!PlayerCache.ContainsKey(player!))
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["player.no_data"]}");
            Logger.LogWarning($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) tried to use command without data");
            return;
        }

        if (!WebManager!.IsAvailable)
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["services.no_services"]}");
            return;
        }

        if (commandInfo.ArgCount < 1)
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["services.invalid_command"]}");
            return;
        }

        string planIdString = commandInfo.GetArg(1);
        if (!int.TryParse(planIdString, out int planId))
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["services.invalid_command"]}");
            return;
        }

        ServiceSmsData? service = WebManager!.GetService(planId);
        if (service == null)
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["services.invalid_command"]}");
            return;
        }

        string instruction = $"{Config.Settings.Prefix}{Localizer["services.how_to_buy_sms",
                       service.Name, service.Count, service.Unit, service.SmsMessage, service.SmsNumber, service.SmsCodeValue, planId]}";

        List<string> lines = GetLines(instruction);

        foreach (var line in lines)
        {
            commandInfo.ReplyToCommand(line);
        }
    }

    [ConsoleCommand("css_kupsms", "Buys service")]
    public void OnBuyCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!Player.IsValid(player))
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["player.invalid"]}");
            return;
        }

        if (!PlayerCache.ContainsKey(player!))
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["player.no_data"]}");
            Logger.LogWarning($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) tried to use command without data");
            return;
        }

        if (!WebManager!.IsAvailable)
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["services.no_services"]}");
            return;
        }

        if (commandInfo.ArgCount < 2)
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["services.buy_sms_invalid_command"]}");
            return;
        }

        string planIDString = commandInfo.GetArg(1);

        if (!int.TryParse(planIDString, out int planID))
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["services.buy_sms_invalid_command"]}");
            return;
        }

        string smsCode = commandInfo.GetArg(2);

        var steamId64 = player!.AuthorizedSteamID!.SteamId64;
        ServiceSmsData? data = WebManager!.GetService(planID);

        if (data == null)
        {
            commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["services.buy_sms_invalid_command"]}");
            return;
        }

        string playerIP = player!.IpAddress!.Split(':')[0];
        string playerName = player.PlayerName;

        Task.Run(async () =>
        {

            bool success = await WebManager!.RegisterServiceBuy(steamId64, data.PlanCode, smsCode, playerIP, playerName);

            if (success)
            {
                Server.NextFrame(() =>
                {
                    commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["ervices.buy_success"]}");

                    Logger.LogInformation($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) bought service {data.Name}({data.PlanCode})");
                });
            }
            else
            {
                Server.NextFrame(() =>
                {
                    commandInfo.ReplyToCommand($"{Config.Settings.Prefix}{Localizer["services.buy_failed"]}");

                    Logger.LogInformation($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) failed to buy service {data.Name}({data.PlanCode}) with: planID={planID} and smsCode={smsCode}");
                });
            }
        });

    }
}


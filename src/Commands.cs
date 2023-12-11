
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
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["player.invalid"]}");
            return;
        }

        if (!PlayerCache.ContainsKey(player!))
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["player.no_data"]}");
            Logger.LogWarning($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) tried to use command without data");
            return;
        }

        if (PlayerCache[player!].ConnectionData.Count > 0)
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["uslugi.your_services"]}");
            for (int i = 0; i < PlayerCache[player!].ConnectionData.Count; i++)
            {
                DateTime dateTime = PlayerCache[player!].ConnectionData[i].End;
                string formatedTime = dateTime.ToString(Localizer["uslugi.datetime_format"]);

                commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["uslugi.entry", i + 1, formatedTime]}");
            }
        }
    }

    [ConsoleCommand("css_sklepsms", "Main shop menu")]
    public void OnShopCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!Player.IsValid(player))
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["player.invalid"]}");
            return;
        }

        if (!PlayerCache.TryGetValue(player!, out var cache))
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["player.no_data"]}");
            Logger.LogWarning($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) tried to use command without data");
            return;
        }

        if (!WebManager!.IsAvailable)
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["services.no_services"]}");
            return;
        }

        commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["sklepsms.shop_www", Config.Sklepcs.WebsiteURL]}");
        if (cache.IsLoadedSklepcs)
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["sklepsms.shop_money", cache.SklepcsMoney, WebManager.CurrencyName]}");
        }

        commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["sklepsms.available", WebManager.Services.Count]}");

        for (int i = 0; i < WebManager.Services.Count; i++)
        {
            var service = WebManager.Services[i];
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["sklepsms.service_entry",
               i + 1, service.Name, service.Count, service.Unit, service.PlanValue / (float)100, WebManager.CurrencyName, service.SmsCodeValue]}");
        }

        commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["sklepsms.how_to_buy_wallet"]}");
        commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["sklepsms.how_to_buy_sms"]}");
    }

    [ConsoleCommand("css_kupsrodkami", "Buys service via wallet money")]
    public void OnBuyWalletCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!Player.IsValid(player))
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["player.invalid"]}");
            return;
        }

        if (!PlayerCache.ContainsKey(player!))
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["player.no_data"]}");
            Logger.LogWarning($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) tried to use command without data");
            return;
        }

        if (!WebManager!.IsAvailable)
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["services.no_services"]}");
            return;
        }

        if (commandInfo.ArgCount < 1)
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["sklepsms.how_to_buy_wallet"]}");
            return;
        }

        string planIDString = commandInfo.GetArg(1);

        if (!int.TryParse(planIDString, out int planID))
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["sklepsms.how_to_buy_wallet"]}");
            return;
        }


        var steamId64 = player!.AuthorizedSteamID!.SteamId64;
        ServiceSmsData? data = WebManager!.GetService(planID);

        if (data == null)
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["sklepsms.how_to_buy_wallet"]}");
            return;
        }

        string playerIP = player!.IpAddress!.Split(':')[0];
        string playerName = player.PlayerName;

        Task.Run(async () =>
        {
            string smsCode = "";
            bool success = await WebManager!.RegisterServiceBuy(steamId64, data.PlanCode, smsCode, playerIP, playerName);

            if (success)
            {
                Server.NextFrame(() =>
                {
                    commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["services.buy_success", data.Name, data.Count, data.Unit]}");

                    Logger.LogInformation($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) bought service {data.Name}({data.PlanCode})");
                });
            }
            else
            {
                Server.NextFrame(() =>
                {
                    commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["services.buy_failed"]}");

                    Logger.LogInformation($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) failed to buy service {data.Name}({data.PlanCode}) with: planID={planID} and smsCode={smsCode}");
                });
            }
        });

    }

    [ConsoleCommand("css_kupsms", "")]
    public void OnShopBuyCommmand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!Player.IsValid(player))
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["player.invalid"]}");
            return;
        }

        if (!PlayerCache.ContainsKey(player!))
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["player.no_data"]}");
            Logger.LogWarning($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) tried to use command without data");
            return;
        }

        if (!WebManager!.IsAvailable)
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["services.no_services"]}");
            return;
        }

        if (commandInfo.ArgCount < 1)
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["sklepsms.how_to_buy_sms"]}");
            return;
        }

        string planIdString = commandInfo.GetArg(1);
        if (!int.TryParse(planIdString, out int planId))
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["sklepsms.how_to_buy_sms"]}");
            return;
        }

        ServiceSmsData? service = WebManager!.GetService(planId);
        if (service == null)
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["sklepsms.how_to_buy_sms"]}");
            return;
        }

        string instruction = $"{PluginChatPrefix}{Localizer["kupsms.instructions",
                       service.Name, service.Count, service.Unit, service.SmsMessage, service.SmsNumber, service.SmsCodeValue, planId]}";

        List<string> lines = GetLines(instruction);

        foreach (var line in lines)
        {
            commandInfo.ReplyToCommand(line);
        }
    }





    [ConsoleCommand("css_kodsms", "Buys service via sms")]
    public void OnBuySmsCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!Player.IsValid(player))
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["player.invalid"]}");
            return;
        }

        if (!PlayerCache.ContainsKey(player!))
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["player.no_data"]}");
            Logger.LogWarning($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) tried to use command without data");
            return;
        }

        if (!WebManager!.IsAvailable)
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["services.no_services"]}");
            return;
        }

        if (commandInfo.ArgCount < 2)
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["kodsms.syntax"]}");
            return;
        }

        string planIDString = commandInfo.GetArg(1);

        if (!int.TryParse(planIDString, out int planID))
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["kodsms.syntax"]}");
            return;
        }

        string smsCode = commandInfo.GetArg(2);

        var steamId64 = player!.AuthorizedSteamID!.SteamId64;
        ServiceSmsData? data = WebManager!.GetService(planID);

        if (smsCode == "" || data == null)
        {
            commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["kodsms.syntax"]}");
            return;
        }

        string playerIP = player!.IpAddress!.Split(':')[0];
        string playerName = player.PlayerName;

        Task.Run(async () =>
        {

            bool success = await WebManager!.RegisterServiceBuy(steamId64, data.PlanCode, smsCode, playerIP, playerName);
            // IF smsCode is empty then player money in walllet is used
            if (success)
            {
                Server.NextFrame(() =>
                {
                    commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["services.buy_success", data.Name, data.Count, data.Unit]}");

                    Logger.LogInformation($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) bought service {data.Name}({data.PlanCode})");
                });
            }
            else
            {
                Server.NextFrame(() =>
                {
                    commandInfo.ReplyToCommand($"{PluginChatPrefix}{Localizer["services.buy_failed"]}");

                    Logger.LogInformation($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) failed to buy service {data.Name}({data.PlanCode}) with: planID={planID} and smsCode={smsCode}");
                });
            }
        });

    }
}



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
        HandlePlayerCommand(player, commandInfo, () =>
        {
            if (PlayerCache[player!].ConnectionData.Count > 0)
            {
                player!.PrintToChat($"{PluginChatPrefix}{Localizer["uslugi.your_services"]}");
                for (int i = 0; i < PlayerCache[player!].ConnectionData.Count; i++)
                {
                    DateTime dateTime = PlayerCache[player!].ConnectionData[i].End;
                    string formatedTime = dateTime.ToString(Localizer["uslugi.datetime_format"]);

                    player!.PrintToChat($"{PluginChatPrefix}{Localizer["uslugi.entry", i + 1, formatedTime]}");
                }
            }
        });
    }

    [ConsoleCommand("css_sklepsms", "Main shop menu")]
    public void OnShopCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        HandlePlayerCommand(player, commandInfo, () =>
        {

            if (!WebManager!.IsAvailable)
            {
                player!.PrintToChat($"{PluginChatPrefix}{Localizer["services.no_services"]}");
                return;
            }

            player!.PrintToChat($"{PluginChatPrefix}{Localizer["sklepsms.shop_www", Config.Sklepcs.WebsiteURL]}");
            if (!PlayerCache[player!].IsLoadedSklepcs)
            {
                player!.PrintToChat($"{PluginChatPrefix}{Localizer["sklepsms.shop_money", PlayerCache[player!].SklepcsMoney, WebManager.CurrencyName]}");
            }

            player!.PrintToChat($"{PluginChatPrefix}{Localizer["sklepsms.available", WebManager.Services.Count]}");

            foreach (var (service, index) in WebManager.Services.Select((s, i) => (s, i + 1)))
            {
                player!.PrintToChat($"{PluginChatPrefix}{Localizer["sklepsms.service_entry",
                    index, service.Name, service.Amount, service.Unit, service.PlanValue / (float)100, WebManager.CurrencyName, service.SmsCost]}");
            }

            player!.PrintToChat($"{PluginChatPrefix}{Localizer["sklepsms.how_to_buy_wallet"]}");
            player!.PrintToChat($"{PluginChatPrefix}{Localizer["sklepsms.how_to_buy_sms"]}");
        });
    }

    [ConsoleCommand("css_kupsrodkami", "Buys service via wallet money")]
    public void OnBuyWalletCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        HandlePlayerCommand(player, commandInfo, () =>
        {

            if (!WebManager!.IsAvailable)
            {
                player!.PrintToChat($"{PluginChatPrefix}{Localizer["services.no_services"]}");
                return;
            }

            if (commandInfo.ArgCount >= 1 && int.TryParse(commandInfo.GetArg(1), out int planID))
            {
                var steamId64 = player!.AuthorizedSteamID!.SteamId64;
                ServicePlanData? data = WebManager!.GetService(planID);

                if (data == null)
                {
                    player!.PrintToChat($"{PluginChatPrefix}{Localizer["sklepsms.how_to_buy_wallet"]}");
                    return;
                }

                string playerIP = player!.IpAddress!.Split(':')[0];
                string playerName = player.PlayerName;
                string smsCode = "";
                Task.Run(async () =>
                {
                    bool success = await WebManager!.RegisterServiceBuy(steamId64, data.PlanUniqueCode, smsCode, playerIP, playerName);

                    if (success)
                    {
                        Server.NextFrame(() =>
                        {
                            player!.PrintToChat($"{PluginChatPrefix}{Localizer["services.buy_success", data.Name, data.Amount, data.Unit]}");

                            if (Config.Settings.IsLoggingLevelEnabled(LoggingLevelData.PurchaseSuccess))
                            {
                                Logger.LogInformation($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) bought service {data.Name}({data.PlanUniqueCode})");
                            }

                        });
                    }
                    else
                    {
                        Server.NextFrame(() =>
                        {
                            player!.PrintToChat($"{PluginChatPrefix}{Localizer["services.buy_failed"]}");

                            if (Config.Settings.IsLoggingLevelEnabled(LoggingLevelData.PurchaseErrors))
                            {
                                Logger.LogInformation($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) failed to buy service {data.Name}({data.PlanUniqueCode}) with: planID={planID} and using SklepCS money");
                            }
                        });
                    }
                });


            }
            else
            {
                player!.PrintToChat($"{PluginChatPrefix}{Localizer["sklepsms.how_to_buy_wallet"]}");
            }
        });

    }

    [ConsoleCommand("css_kupsms", "")]
    public void OnShopBuyCommmand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        HandlePlayerCommand(player, commandInfo, () =>
        {

            if (!WebManager!.IsAvailable)
            {
                player!.PrintToChat($"{PluginChatPrefix}{Localizer["services.no_services"]}");
                return;
            }

            if (commandInfo.ArgCount >= 1 && int.TryParse(commandInfo.GetArg(1), out int planId))
            {
                ServicePlanData? service = WebManager!.GetService(planId);
                if (service == null)
                {
                    player!.PrintToChat($"{PluginChatPrefix}{Localizer["sklepsms.how_to_buy_sms"]}");
                    return;
                }

                string instruction = $"{PluginChatPrefix}{Localizer["kupsms.instructions",
                               service.Name, service.Amount, service.Unit, service.SmsMessage, service.SmsNumber, service.SmsCost, planId]}";

                List<string> lines = GetLines(instruction);

                foreach (var line in lines)
                {
                    player!.PrintToChat(line);
                }


            }
            else
            {
                player!.PrintToChat($"{PluginChatPrefix}{Localizer["sklepsms.how_to_buy_sms"]}");
            }


        });
    }


    [ConsoleCommand("css_kodsms", "Buys service via sms")]
    public void OnBuySmsCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        HandlePlayerCommand(player, commandInfo, () =>
        {

            if (!WebManager!.IsAvailable)
            {
                player!.PrintToChat($"{PluginChatPrefix}{Localizer["services.no_services"]}");
                return;
            }

            if (commandInfo.ArgCount >= 2 && int.TryParse(commandInfo.GetArg(1), out int planID))
            {
                string smsCode = commandInfo.GetArg(2);

                var steamId64 = player!.AuthorizedSteamID!.SteamId64;
                ServicePlanData? data = WebManager!.GetService(planID);

                if (smsCode == "" || data == null)
                {
                    player!.PrintToChat($"{PluginChatPrefix}{Localizer["kodsms.syntax"]}");
                    return;
                }

                string playerIP = player!.IpAddress!.Split(':')[0];
                string playerName = player.PlayerName;

                Task.Run(async () =>
                {

                    bool success = await WebManager!.RegisterServiceBuy(steamId64, data.PlanUniqueCode, smsCode, playerIP, playerName);

                    if (success)
                    {
                        Server.NextFrame(() =>
                        {
                            player!.PrintToChat($"{PluginChatPrefix}{Localizer["services.buy_success", data.Name, data.Amount, data.Unit]}");

                            if (Config.Settings.IsLoggingLevelEnabled(LoggingLevelData.PurchaseSuccess))
                            {
                                Logger.LogInformation($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) bought service {data.Name}({data.PlanUniqueCode})");
                            }

                        });
                    }
                    else
                    {
                        Server.NextFrame(() =>
                        {
                            player!.PrintToChat($"{PluginChatPrefix}{Localizer["services.buy_failed"]}");

                            if (Config.Settings.IsLoggingLevelEnabled(LoggingLevelData.PurchaseErrors))
                            {
                                Logger.LogInformation($"{player!.PlayerName}({player!.AuthorizedSteamID!.SteamId64}) failed to buy service {data.Name}({data.PlanUniqueCode}) with: planID={planID} and smsCode={smsCode}");
                            }
                        });
                    }
                });

            }
            else
            {

                player!.PrintToChat($"{PluginChatPrefix}{Localizer["kodsms.syntax"]}");
            }
        });
    }


    private void HandlePlayerCommand(CCSPlayerController? player, CommandInfo commandInfo, Action action)
    {
        if (!Player.IsValid(player))
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



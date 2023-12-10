using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CounterStrikeSharp.API;

namespace SklepCSManager
{
    enum SklepcsWebOperation
    {
        GetServices = 1,
        GetSettings = 2,
        GetMoney = 3,
    }

    public class SklepcsWebManager
    {
        public string CurrencyName { get; set; } = "wPLN";
        public string ShopWebsite { get; set; } = "sklep.csgejmerzy.pl";
        public string ApiKey { get; set; } = "1234567890";
        public string ApiUrl { get; set; } = "https://sklepcs.pl/";
        public string ServerID { get; set; } = "1";
        public int ApiVersion { get; set; } = 142;

        public List<ServiceSmsData> Services { get; set; } = new List<ServiceSmsData>();

        private async Task<List<string>> QueryApiAsync(SklepcsWebOperation operation, string queryExtraData = "")
        {
            string apiUrl = $"{ApiUrl}api_server_uslugi.php?api={ApiKey}&serwer={ShopWebsite}&ver={ApiVersion}&operacja={(int)operation}" + queryExtraData;

            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    Server.PrintToConsole($"[SklepcsWebManager] Query response: {responseData}");
                    return BreakQuery(responseData);
                }
                else
                {
                    throw new Exception($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }

        public async Task<bool> LoadWebServices()
        {
            try
            {
                var responseList = await QueryApiAsync(SklepcsWebOperation.GetServices);

                // Break the response into lists of 8 values
                for (int i = 0; i < responseList.Count; i += 8)
                {
                    ServiceSmsData service = new ServiceSmsData
                    {
                        Name = responseList[i],
                        Count = int.Parse(responseList[i + 1]),
                        Unit = responseList[i + 2],
                        SmsCodeValue = decimal.Parse(responseList[i + 3]),
                        SmsMessage = responseList[i + 4],
                        SmsCode = responseList[i + 5],
                        PlanCode = responseList[i + 6],
                        PlanValue = int.Parse(responseList[i + 7])
                    };

                    Services.Add(service);
                }

                return true;
            }
            catch (Exception ex)
            {
                // log error to plugin log
                return false;
            }
        }

        public async Task<bool> LoadWebSettings()
        {
            try
            {
                var responseList = await QueryApiAsync(SklepcsWebOperation.GetSettings);

                CurrencyName = responseList[0];
                ShopWebsite = responseList[1];

                return true;
            }
            catch (Exception ex)
            {
                // log error to plugin log
                return false;
            }
        }

        public async Task<int> LoadPlayerMoney(ulong SteamId64)
        {
            string queryExtraData = $"&client={5}&sid64={SteamId64}";

            try
            {
                var responseList = await QueryApiAsync(SklepcsWebOperation.GetMoney, queryExtraData);

                int ClientIndex = int.Parse(responseList[0]);
                int money = int.Parse(responseList[1]); // Money * 100
                return money;
            }
            catch (Exception ex)
            {  
                // LOG error to plugin log
                return -1; 
            }
        }

        public async Task<bool> RegisterServiceBuy(ulong steamId64, string planShortId, string playerIP, string playerName)
        {
            string apiUrl = $"{ApiUrl}api_server.php?api={ApiKey}&steam64={steamId64}&tekst={planShortId}&ip={playerIP}&serwer={ShopWebsite}&ver={ApiVersion}&client={5}&name={playerName}";

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        return responseData.ToLower().Contains("ok");
                    }
                    else
                    {
                        // TO:DO log error to plugin log
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    // TO:DO log error to plugin log
                    return false;
                }
            }
        }

        private List<string> BreakQuery(string query)
        {
            return new List<string>(query.Split(';'));
        }
    }
}

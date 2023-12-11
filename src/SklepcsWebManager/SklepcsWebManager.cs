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
        private string ApiKey { get; set; } = "1234567890";
        private string ApiUrl { get; set; } = "https://sklepcs.pl/";
        private string _serverID { get; set; } = "1";
        private int ApiVersion { get; set; } = 142;

        public List<ServiceSmsData> Services { get; set; } = new List<ServiceSmsData>();

        private bool _servicesLoaded = false;
        private bool _settingsLoaded = false;


        public bool IsLoaded => _servicesLoaded && _settingsLoaded;

        public SklepcsWebManager(string serverID, string apiKey)
        {
            _serverID = serverID;
            ApiKey = apiKey;
        }

        private async Task<List<string>> QueryApiAsync(SklepcsWebOperation operation, string queryExtraData = "")
        {
            string apiUrl = $"{ApiUrl}api_server_uslugi.php?api={ApiKey}&serwer={_serverID}&ver={ApiVersion}&operacja={(int)operation}" + queryExtraData;

            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                 
                    return BreakQuery(responseData);
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        public async Task<bool> LoadWebServices()
        {
            try
            {
                var responseList = await QueryApiAsync(SklepcsWebOperation.GetServices);

                // Break the response into lists of 8 values
                for (int i = 0; i + 8 < responseList.Count; i += 8)
                {
                    ServiceSmsData service = new ServiceSmsData
                    {
                        Name = responseList[i],
                        Count = int.Parse(responseList[i + 1]),
                        Unit = responseList[i + 2],
                        SmsCodeValue = decimal.Parse(responseList[i + 3]),
                        SmsMessage = responseList[i + 4],
                        SmsNumber = responseList[i + 5],
                        PlanCode = responseList[i + 6],
                        PlanValue = int.Parse(responseList[i + 7])
                    };

                    Services.Add(service);
                }
                _servicesLoaded = true;
                return true;
            }
            catch (Exception ex)
            {
                
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

                _settingsLoaded = true;
                return true;
            }
            catch (Exception ex)
            {
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
                return -1;
            }
        }

        public async Task<bool> RegisterServiceBuy(ulong steamId64, string planShortId, string SmsCode, string playerIP, string playerName)
        {
            string apiUrl = $"{ApiUrl}api_server.php?api={ApiKey}&steam64={steamId64}&tekst={planShortId + '-' + SmsCode}&ip={playerIP}&serwer={_serverID}&ver={ApiVersion}&client={5}&name={playerName}";

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
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public ServiceSmsData? GetService(int planID)
        {
            if (planID < 0 || planID > Services.Count)
            {
                return null;
            }

            return Services[planID - 1];
        }

        public async Task<bool> AddPlayerFlags()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AddPlayerMoney()
        {
            throw new NotImplementedException();
        }

        private List<string> BreakQuery(string query)
        {
            return new List<string>(query.Split(';'));
        }
    }
}

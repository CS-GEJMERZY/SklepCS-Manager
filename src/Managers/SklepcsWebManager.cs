using Plugin.Models;

namespace Plugin.Managers
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
        public string ShopWebsite { get; set; } = "sklep.csgejmerzy.pl"; // consider removing this
        private string ApiKey { get; set; } = "1234567890";
        private int ApiVersion { get; set; } = 142;
        private string ApiUrl { get; set; } = "https://sklepcs.pl/";
        private string ServerId { get; set; } = "1";
        public List<ServicePlanData> Services { get; set; } = new List<ServicePlanData>();

        private bool _servicesLoaded = false;
        private bool _settingsLoaded = false;


        public bool IsAvailable => _servicesLoaded && _settingsLoaded && Services.Count > 0;

        public SklepcsWebManager(string serverID, string apiKey)
        {
            ServerId = serverID;
            ApiKey = apiKey;
        }

        private async Task<List<string>> QueryServerApisync(SklepcsWebOperation operation, string queryExtraData = "")
        {
            string apiUrl = $"{ApiUrl}api_server_uslugi.php?api={ApiKey}&serwer={ServerId}&ver={ApiVersion}&operacja={(int)operation}" + queryExtraData;


            using HttpClient httpClient = new();
            HttpResponseMessage response;
            try
            {
                response = await httpClient.GetAsync(apiUrl);
            }
            catch (Exception ex)
            {
                throw new Exception("Error occured while querying server api.", ex);
            }


            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();

                return BreakQueryLines(responseData);
            }
            else
            {
                throw new Exception($"Response status code: {response.StatusCode}. Url: {apiUrl} ");
            }
        }

        public async Task LoadWebServices()
        {
            try
            {
                var responseList = await QueryServerApisync(SklepcsWebOperation.GetServices);

                ParseServiceResponse(responseList);

                _servicesLoaded = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not load web services:", ex);
            }
        }

        public async Task LoadWebSettings()
        {
            try
            {
                var responseList = await QueryServerApisync(SklepcsWebOperation.GetSettings);

                ParseSettingsResponse(responseList);

                _settingsLoaded = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error occured while loading web settings.", ex);
            }
        }

        public async Task<int> LoadPlayerMoney(ulong SteamId64)
        {
            string queryExtraData = $"&client={5}&sid64={SteamId64}";

            try
            {
                var responseList = await QueryServerApisync(SklepcsWebOperation.GetMoney, queryExtraData);

                int ClientIndex = int.Parse(responseList[0]);
                int money = int.Parse(responseList[1]); // Money * 100
                return money;
            }
            catch (Exception ex)
            {
                throw new Exception("Error occured while loading player money.", ex);
            }
        }

        // true if purchase is succesful, false otherwise
        public async Task<bool> RegisterServiceBuy(ulong steamId64, string planShortId, string SmsCode, string playerIP, string playerName)
        {
            string apiUrl = $"{ApiUrl}api_server.php?api={ApiKey}&steam64={steamId64}&tekst={planShortId + '-' + SmsCode}&ip={playerIP}&serwer={ServerId}&ver={ApiVersion}&client={5}&name={playerName}";

            using HttpClient httpClient = new();
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
                throw new Exception("Error occured while registering service buy.", ex);
            }
        }

        public async Task<bool> AddPlayerFlags()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AddPlayerMoney()
        {
            throw new NotImplementedException();
        }

        public ServicePlanData? GetService(int planID)
        {
            return planID < 0 || planID > Services.Count ? null : Services[planID - 1];
        }

        private static List<string> BreakQueryLines(string query)
        {
            return new List<string>(query.Split(';'));
        }

        private void ParseServiceResponse(List<string> responseList)
        {
            const int FieldsPerService = 8;

            for (int i = 0; i + FieldsPerService < responseList.Count; i += FieldsPerService)
            {
                ServicePlanData service = new()
                {
                    Name = responseList[i],
                    Amount = int.Parse(responseList[i + 1]),
                    Unit = responseList[i + 2],
                    SmsCost = responseList[i + 3],
                    SmsMessage = responseList[i + 4],
                    SmsNumber = responseList[i + 5],
                    PlanUniqueCode = responseList[i + 6],
                    PlanValue = int.Parse(responseList[i + 7])
                };

                Services.Add(service);
            }
        }

        private void ParseSettingsResponse(List<string> responseList)
        {
            CurrencyName = responseList[0];
            ShopWebsite = responseList[1];
        }
    }
}


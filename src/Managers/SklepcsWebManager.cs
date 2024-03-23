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
        private string _shopWebsite { get; set; } = "sklep.csgejmerzy.pl"; // consider removing this
        private string _apiKey { get; set; } = "1234567890";
        private string _apiUrl { get; set; } = "https://sklepcs.pl/";
        private string _serverId { get; set; } = "1";
        private int _apiVersion { get; set; } = 142;

        private string _lastQueryString { get; set; }
        private string _lastQueryResponse { get; set; }
        private string _lastException { get; set; }


        public List<ServicePlanData> Services { get; set; } = new List<ServicePlanData>();

        private bool _servicesLoaded = false;
        private bool _settingsLoaded = false;


        public bool IsAvailable => _servicesLoaded && _settingsLoaded && Services.Count > 0;

        public SklepcsWebManager(string serverID, string apiKey)
        {
            _serverId = serverID;
            _apiKey = apiKey;

            _lastQueryString = "";
            _lastQueryResponse = "";
            _lastException = "";
        }

        private async Task<List<string>> QueryServerApisync(SklepcsWebOperation operation, string queryExtraData = "")
        {
            string apiUrl = $"{_apiUrl}api_server_uslugi.php?api={_apiKey}&serwer={_serverId}&ver={_apiVersion}&operacja={(int)operation}" + queryExtraData;
            _lastQueryString = apiUrl;

            using HttpClient httpClient = new();
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                _lastQueryResponse = responseData;

                return BreakQueryLines(responseData);
            }
            else
            {
                return new List<string>();
            }
        }

        public async Task<bool> LoadWebServices()
        {
            try
            {
                var responseList = await QueryServerApisync(SklepcsWebOperation.GetServices);

                ParseServiceResponse(responseList);

                _servicesLoaded = true;
                return true;
            }
            catch (Exception ex)
            {
                _lastException = ex.Message;
                return false;
            }
        }

        public async Task<bool> LoadWebSettings()
        {
            try
            {
                var responseList = await QueryServerApisync(SklepcsWebOperation.GetSettings);

                ParseSettingsResponse(responseList);

                _settingsLoaded = true;
                return true;
            }
            catch (Exception ex)
            {
                _lastException = ex.Message;
                return false;
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
                _lastException = ex.Message;
                return -1;
            }
        }

        public async Task<bool> RegisterServiceBuy(ulong steamId64, string planShortId, string SmsCode, string playerIP, string playerName)
        {
            string apiUrl = $"{_apiUrl}api_server.php?api={_apiKey}&steam64={steamId64}&tekst={planShortId + '-' + SmsCode}&ip={playerIP}&serwer={_serverId}&ver={_apiVersion}&client={5}&name={playerName}";

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
                _lastException = ex.Message;
                return false;
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

        public string GetDebugData()
        {
            string reponse = $"Last query: {_lastQueryString}\nLast response: {_lastQueryResponse}\nLast exception: {_lastException}";
            return reponse;
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
            _shopWebsite = responseList[1];
        }
    }


}


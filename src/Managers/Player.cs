using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;


namespace Plugin.Managers
{
    public class Player
    {
        public List<Models.PlayerDatabaseData> ConnectionData { get; private set; }
        public List<string> AddedPermissions { get; private set; }

        private bool _loadedDatabaseData = false;
        public bool _loadedSklepcsData { get; set; } = false;


        public bool IsLoadedDatabase => _loadedDatabaseData;
        public bool IsLoadedSklepcs => _loadedSklepcsData;

        public bool IsFullyLoaded => _loadedDatabaseData && _loadedSklepcsData;

        public double SklepcsMoney { get; set; } = 0;

        public Player()
        {
            ConnectionData = new List<Models.PlayerDatabaseData>();
            AddedPermissions = new List<string>();
        }

        public static bool IsValid(CCSPlayerController? player)
        {
            return player != null && player.IsValid && !player.IsBot && !player.IsHLTV;
        }


        public async Task LoadDatabaseData(string SteamID2, string serverTag, DatabaseManager databaseManager)
        {
            ConnectionData = await databaseManager.FetchPlayerData(SteamID2, serverTag);
            _loadedDatabaseData = true;
        }

        public async Task<bool> LoadSklepcsData(ulong SteamId64, SklepcsWebManager webManager)
        {
            int money = await webManager.LoadPlayerMoney(SteamId64);

            if (SklepcsMoney == -1)
            {
                _loadedSklepcsData = false;
                return false;
            }

            float moneyFloat = (float)money / 100;
            SklepcsMoney = Math.Round(moneyFloat, 2);

            _loadedSklepcsData = true;
            return true;
        }

        public void AssignPermissions(CCSPlayerController player, PermissionManager PermisisonManager)
        {
            List<string> fetchedPermissions = PermisisonManager.FetchPermissions(ConnectionData);

            foreach (string permission in fetchedPermissions)
            {
                AddedPermissions.Add(permission);
                AdminManager.AddPlayerPermissions(player, permission);
            }
        }
    }
}

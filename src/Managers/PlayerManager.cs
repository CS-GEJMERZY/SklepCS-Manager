using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;


namespace Plugin.Managers
{
    public class PlayerManager
    {
        public List<Models.PlayerDatabaseData> ConnectionData { get; private set; }
        public List<string> AddedPermissions { get; private set; }

        private bool _loadedDatabaseData = false;
        public bool LoadedSklepcsData { get; set; } = false;


        public bool IsLoadedDatabase => _loadedDatabaseData;
        public bool IsLoadedSklepcs => LoadedSklepcsData;

        public bool IsFullyLoaded => _loadedDatabaseData && LoadedSklepcsData;

        public double SklepcsMoney { get; set; } = 0;

        public PlayerManager()
        {
            ConnectionData = [];
            AddedPermissions = [];
        }

        public static bool IsValid(CCSPlayerController? player)
        {
            return player != null && player.IsValid && !player.IsBot && !player.IsHLTV;
        }


        public async Task LoadDatabaseData(string SteamID2, string serverTag, DatabaseManager databaseManager)
        {
            try
            {
                ConnectionData = await databaseManager.FetchPlayerData(SteamID2, serverTag);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load player datase data. ", ex);
            }
            _loadedDatabaseData = true;
        }

        public async Task<bool> LoadSklepcsData(ulong SteamId64, SklepcsWebManager webManager)
        {

            int money;
            try
            {

                money = await webManager.LoadPlayerMoney(SteamId64);
            }
            catch (Exception)
            {
                throw;
            }

            if (SklepcsMoney == -1)
            {
                LoadedSklepcsData = false;
                return false;
            }

            float moneyFloat = (float)money / 100;
            SklepcsMoney = Math.Round(moneyFloat, 2);

            LoadedSklepcsData = true;
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

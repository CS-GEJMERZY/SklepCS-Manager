using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

public class Player
{
    public List<PlayerConnectionData> ConnectionData { get; private set; }
    public List<string> AddedPermissions { get; private set; }

    private bool _loadedDatabaseData = false;

    public bool IsLoadedDatabase => _loadedDatabaseData;

    public Player()
    {
        ConnectionData = new List<PlayerConnectionData>();
        AddedPermissions = new List<string>();
    }

    public static bool IsValid(CCSPlayerController? player)
    {
        return player != null && player.IsValid && !player.IsBot && !player.IsHLTV;
    }


    public async Task LoadDatabaseData(string SteamID2, string serverTag, SklepcsDatabaseManager databaseManager)
    {
        ConnectionData = await databaseManager.FetchPlayerData(SteamID2, serverTag);
        _loadedDatabaseData = true;
    }

    public void LoadPermissions(CCSPlayerController player, SklepcsPermissionManager PermisisonManager)
    {
        List<string> fetchedPermissions = PermisisonManager.FetchPermissions(ConnectionData);

        foreach (string permission in fetchedPermissions)
        {
            AddedPermissions.Add(permission);
            AdminManager.AddPlayerPermissions(player, permission);
        }
    }
}
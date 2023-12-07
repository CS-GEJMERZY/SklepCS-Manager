using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;

public class Player
{
    public List<PlayerConnectionData> ConnectionData { get; private set; }
    public List<string> AddedPermissions { get; private set; }

    private bool _loadedDatabaseData = false;

    public bool IsValid => _loadedDatabaseData;

    public Player()
    {
        ConnectionData = new List<PlayerConnectionData>();
        AddedPermissions = new List<string>();
    }

    public void LoadDatabaseData(CCSPlayerController player, SklepcsDatabaseManager databaseManager, string serverTag)
    {
        if (player == null)
        {
            return;
        }

        ConnectionData = databaseManager.FetchPlayerData(player, serverTag);
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
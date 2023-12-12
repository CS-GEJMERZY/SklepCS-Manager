namespace SklepCSManager;

public class PermissionManager
{
    public List<SklepcsPermission> Permissions { get; }

    public PermissionManager(List<SklepcsPermission> permissionsConfig)
    {
        Permissions = permissionsConfig;
    }

    public List<string> FetchPermissions(List<PlayerDatabaseData> playerConnectionData)
    {
        List<string> matchedPermissions = new();
        HashSet<char> rawPlayerFlags = new();

        foreach (PlayerDatabaseData connectionData in playerConnectionData)
        {
            foreach (char c in connectionData.Flags)
            {
                rawPlayerFlags.Add(c);
            }
        }

        foreach (SklepcsPermission group in Permissions)
        {
            if (group.RequiredFlags.All(flag => rawPlayerFlags.Contains(flag)))
            {
                matchedPermissions.AddRange(group.Permissions);
            }
        }

        return matchedPermissions;
    }
}
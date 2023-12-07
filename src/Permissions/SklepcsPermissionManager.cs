public class SklepcsPermissionManager
{
    public List<SklepcsPermission> Permissions { get; }

    public SklepcsPermissionManager(List<SklepcsPermission> permissionsConfig)
    {
        Permissions = permissionsConfig;
    }

    public List<string> FetchPermissions(List<PlayerConnectionData> playerConnectionData)
    {
        List<string> matchedPermissions = new();
        HashSet<char> rawPlayerFlags = new();

        foreach (PlayerConnectionData connectionData in playerConnectionData)
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
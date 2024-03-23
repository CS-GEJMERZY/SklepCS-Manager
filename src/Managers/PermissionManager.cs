using Plugin.Models;

namespace Plugin.Managers
{
    public class PermissionManager
    {
        public List<Configs.SklepcsPermissionData> Permissions { get; }

        public PermissionManager(List<Configs.SklepcsPermissionData> permissionsConfig)
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

            foreach (Configs.SklepcsPermissionData group in Permissions)
            {
                if (group.RequiredFlags.All(flag => rawPlayerFlags.Contains(flag)))
                {
                    matchedPermissions.AddRange(group.Permissions);
                }
            }

            return matchedPermissions;
        }
    }
}


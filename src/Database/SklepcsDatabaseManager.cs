using CounterStrikeSharp.API.Core;
using MySqlConnector;

public class SklepcsDatabaseManager
{
    internal string ConnectionsString = string.Empty;
    internal MySqlConnection Conneciton;

    public SklepcsDatabaseManager(DatabaseData databaseConfig)
    {
        MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
        {
            Server = databaseConfig.DBHostname,
            Database = databaseConfig.DBDatabase,
            UserID = databaseConfig.DBUser,
            Password = databaseConfig.DBPassword,
            Port = databaseConfig.DBPort,
        };

        ConnectionsString = builder.ConnectionString;

        try
        {
            Conneciton = new MySqlConnection(ConnectionsString);
            Conneciton.Open();
        }
        catch (Exception ex)
        {
            throw new Exception($"Database is inaccesible. Error: {ex.Message}");
        }
    }

    public List<PlayerConnectionData> FetchPlayerData(CCSPlayerController player, string serverTag)
    {
        if (Conneciton == null)
        {
            throw new Exception("Database is null, but tried to query player data.");
        }


        List<PlayerConnectionData> PlayerDataList = new List<PlayerConnectionData>();

        if (player.AuthorizedSteamID == null)
        {
            return PlayerDataList;
        }


        string query = $"SELECT authtype, flags, immunity, serwer, koniec FROM sklepcs_vip WHERE identity = '{player.AuthorizedSteamID.SteamId2}' and serwer = '{serverTag}'";

        try
        {
            using var command = new MySqlCommand(query, this.Conneciton);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                PlayerConnectionData playerData = new PlayerConnectionData
                {
                    AuthType = reader.GetString("authtype"),
                    Flags = reader.GetString("flags"),
                    Immunity = reader.GetInt32("immunity"),
                    End = reader.GetDateTime("koniec")
                };

                PlayerDataList.Add(playerData);

            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Encountered error while reading player data from database: {ex.Message}");
        }

        return PlayerDataList;
    }
}

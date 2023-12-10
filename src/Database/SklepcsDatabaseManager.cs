using System.Data;
using MySqlConnector;

public class SklepcsDatabaseManager
{
    internal string ConnectionsString = string.Empty;
    internal MySqlConnection Connection;

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
            Connection = new MySqlConnection(ConnectionsString);
        }
        catch (Exception ex)
        {
            throw new Exception($"Database is inaccesible. Error: {ex.Message}");
        }
    }

    public async Task<List<PlayerConnectionData>> FetchPlayerData(string SteamId2, string serverTag)
    {
        if (Connection == null)
        {
            throw new Exception("Database is null, but tried to query player data.");
        }

        List<PlayerConnectionData> playerDataList = new List<PlayerConnectionData>();

        using (var command = new MySqlCommand())
        {
            try
            {
                await Connection.OpenAsync();

                string query = $"SELECT authtype, flags, immunity, serwer, koniec FROM sklepcs_vip WHERE identity = '{SteamId2}' AND (serwer = '{serverTag}' or serwer='all')";

                command.CommandText = query;
                command.Connection = Connection;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PlayerConnectionData playerData = new PlayerConnectionData
                        {
                            AuthType = reader.GetString("authtype"),
                            Flags = reader.GetString("flags"),
                            Immunity = reader.GetInt32("immunity"),
                            End = reader.GetDateTime("koniec")
                        };

                        playerDataList.Add(playerData);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Encountered error while reading player data from the database: {ex.Message}");
            }
            finally
            {
                if (Connection.State == ConnectionState.Open)
                {
                    await Connection.CloseAsync();
                }
            }
        }

        return playerDataList;
    }
}

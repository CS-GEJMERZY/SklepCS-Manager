using System.Data;
using MySqlConnector;
using Plugin.Models;

namespace Plugin.Managers
{
    public class DatabaseManager
    {
        private string ConnectionsString = string.Empty;
        private MySqlConnection Connection;

        public DatabaseManager(Configs.DatabaseData databaseConfig)
        {
            MySqlConnectionStringBuilder builder = new()
            {
                Server = databaseConfig.DBHostname,
                Database = databaseConfig.DBDatabase,
                UserID = databaseConfig.DBUser,
                Password = databaseConfig.DBPassword,
                Port = databaseConfig.DBPort,
            };

            ConnectionsString = builder.ConnectionString;

            Connection = new MySqlConnection(ConnectionsString);
        }

        public async Task<List<PlayerDatabaseData>> FetchPlayerData(string SteamId2, string serverTag)
        {
            if (Connection == null)
            {
                throw new Exception("Database is null, but tried to query player data.");
            }

            List<PlayerDatabaseData> playerDataList = new List<PlayerDatabaseData>();

            using (var command = new MySqlCommand())
            {
                try
                {
                    await Connection.OpenAsync();

                    string query = $"SELECT authtype, flags, immunity, serwer, koniec FROM sklepcs_vip WHERE identity = '{SteamId2}' AND (serwer = '{serverTag}' or serwer='all')";

                    command.CommandText = query;
                    command.Connection = Connection;

                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        PlayerDatabaseData playerData = new PlayerDatabaseData
                        {
                            AuthType = reader.GetString("authtype"),
                            Flags = reader.GetString("flags"),
                            Immunity = reader.GetInt32("immunity"),
                            End = reader.GetDateTime("koniec")
                        };

                        playerDataList.Add(playerData);
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
}


using MySqlConnector;
using Plugin.Models;

namespace Plugin.Managers
{
    public class DatabaseManager
    {
        private readonly string ConnectionsString = string.Empty;

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
        }

        public async Task<List<PlayerDatabaseData>> FetchPlayerData(string SteamId2, string serverTag)
        {
            List<PlayerDatabaseData> playerDataList = [];

            try
            {
                using var Connection = new MySqlConnection(ConnectionsString);
                await Connection.OpenAsync();

                using var command = new MySqlCommand();
                string query = $"SELECT authtype, flags, immunity, serwer, koniec FROM sklepcs_vip WHERE identity = '{SteamId2}' AND (serwer = '{serverTag}' or serwer='all')";

                command.CommandText = query;
                command.Connection = Connection;

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    PlayerDatabaseData playerData = new()
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
                throw new Exception($"Encountered error while reading player data from the database.", ex);
            }

            return playerDataList;
        }
    }
}


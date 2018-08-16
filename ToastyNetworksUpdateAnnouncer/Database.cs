using MySql.Data.MySqlClient;

namespace ToastyNetworksUpdateAnnouncer
{
    public class Database
    {
        static ConnectionBuilder connectionBuilder = new ConnectionBuilder();

        public static MySqlConnection GetConnectionString()
        {
            return connectionBuilder.ConnectionString();
        }
    }
}
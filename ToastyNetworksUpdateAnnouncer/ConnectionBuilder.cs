using MySql.Data.MySqlClient;

namespace ToastyNetworksUpdateAnnouncer
{
    public class ConnectionBuilder
    {
        private string Database = "";
        private string Server = "";
        private string User = "";
        private string Password = "";

        public MySqlConnection ConnectionString()
        {
            return new MySqlConnection($"Server={Server};Database={Database};UID={User};Password={Password};SslMode=none");
        }
    }
}
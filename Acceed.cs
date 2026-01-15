using System.Data;
using System.Data.OleDb;

namespace Citation
{
    class Acceed
    {
        public OleDbConnection Connection { get; set; }

        public static Acceed Shared { get; set; } = new Acceed("fuckSDAU");
        private string password = "Me+h0d2025S0ftwaR1";
        private string? _Path;

        public Acceed(string dbPath)
        {
            if (dbPath == "fuckSDAU") return;

            _Path = dbPath;

            var connectString = App.EnableSecurity ? $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};" : $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};";
            Connection = new OleDbConnection(connectString);
            Connection.Open();
        }

        public void ReConnect(string dbPath)
        {
            // This part of fault tolerance
            if (Connection is null)
            {

                var reconnectString = App.EnableSecurity
                    ? $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};"
                    : $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};";
                Connection = new OleDbConnection(reconnectString);
                Connection.Open();
                return;
            }

            if (_Path == dbPath && Connection.State != ConnectionState.Open)
            {
                Connection.Open();
                return;
            }

            _Path = dbPath;
            Connection?.Close();

            var connectString = App.EnableSecurity ? $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Jet OLEDB:Database Password={password};" : $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};";
            Connection = new OleDbConnection(connectString);
            Connection.Open();
        }

        public void Close()
        {
            Connection?.Close();
        }

        public OleDbDataReader Query(string queryString)
        {
            if (Connection?.State is ConnectionState.Closed) Connection.Open();
            using var command = new OleDbCommand(queryString, Connection);
            var reader = command.ExecuteReader();
            return reader;
        }

        public int Execute(string commandString)
        {
            if (Connection?.State is ConnectionState.Closed) Connection.Open();
            using var command = new OleDbCommand(commandString, Connection);

            // Access databases are incredibly fun, you know?
            return command.ExecuteNonQuery();
        }
    }
}
using System.Data.OleDb;

namespace Citation
{
    class Acceed
    {
        private OleDbConnection? _connection = null;

        public static Acceed Shared { get; set; } = new Acceed("fuckSDAU");

        private string _Path;

        public Acceed(string dbPath)
        {
            if (dbPath == "fuckSDAU") return;

            _Path = dbPath;
            var connectString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath}";
            _connection = new OleDbConnection(connectString);
            _connection.Open();
        }

        public void ReConnect(string dbPath)
        {
            if (_Path == dbPath) return;
            _Path = dbPath;
            _connection?.Close();

            var connectString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath}";
            _connection = new OleDbConnection(connectString);
            _connection.Open();
        }

        public void CloseConnect() => _connection!.Close();

        public OleDbDataReader Query(string queryString)
        {
            using var command = new OleDbCommand(queryString, _connection);
            var reader = command.ExecuteReader();
            return reader;
        }

        public int Insert(string commandString)
        {
            using var command = new OleDbCommand(commandString, _connection);
            return command.ExecuteNonQuery();
        }
    }
}
using Citation.Utils;
using System.ComponentModel;
using System.Data.OleDb;
using System.Windows;

namespace Citation.Model
{
    public class Alert : IDbContext<Alert>, INotifyPropertyChanged
    {
        public Alert() { }

        public Alert(DateTime occurTime, string? title, string? description)
        {
            OccurTime = occurTime;
            Title = title;
            Description = description;
        }

        public DateTime OccurTime
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(OccurTime));
            }
        }

        public string? Title
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public string? Description
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AppendRealtime()
        {
            ToSql(Acceed.Shared.Connection);

            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.Alerts!.Add(this);
        }

        public static Alert FromSql(OleDbDataReader reader)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;

            var title = reader["AlertTitle"].ToString();
            var description = reader["AlertDescription"].ToString();

            var time = DateTime.MaxValue;
            try
            {
                time = DateTime.Parse(Cryptography.DecryptData(
                   mainWindow!.verify,
                   reader["AlertTime"].ToString()!));
            }
            catch (System.FormatException)
            {
                // ignored
            }

            var instance = new Alert(time, title, description);
            return Cryptography.DecryptObject(mainWindow!.verify, instance);
        }

        public void ToSql(OleDbConnection connection)
        {
            var timeString = OccurTime.ToString("yyyy/M/dd HH:mm:ss");
            var sqlCommand = $"""
                INSERT INTO tb_Alert (AlertTitle, AlertDescription, AlertTime)
                VALUES (?, ?, ?)
                """;

            var mainWindow = Application.Current.MainWindow as MainWindow;
            var command = new OleDbCommand(sqlCommand, connection);
            var encrypt = Cryptography.EncryptObject(mainWindow!.verify, this);

            command.Parameters.AddWithValue("?", encrypt.Title);
            command.Parameters.AddWithValue("?", encrypt.Description);
            command.Parameters.AddWithValue("?",
                Cryptography.EncryptData(mainWindow!.verify, timeString));

            command.ExecuteNonQuery();
        }

        public void DeleteSql(OleDbConnection connection)
        {
            var sqlCommand = $"""
                DELETE FROM tb_Alert
                WHERE AlertTitle = ? and AlertDescription = ?
                """;

            var command = new OleDbCommand(sqlCommand, connection);
            var mainWindow = Application.Current.MainWindow as MainWindow;
            var password = mainWindow!.verify;

            command.Parameters.AddWithValue("?", Cryptography.EncryptData(password!, Title!));
            command.Parameters.AddWithValue("?", Cryptography.EncryptData(password!, Description!));

            command.ExecuteNonQuery();
        }
    }
}

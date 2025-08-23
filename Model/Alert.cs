using System.ComponentModel;
using System.Data.OleDb;
using System.Windows;

namespace Citation.Model
{
    public class Alert(DateTime occurTime, string? title, string? description)
        : IDbContext<Alert>, INotifyPropertyChanged
    {
        public DateTime OccurTime
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(OccurTime));
            }
        } = occurTime;

        public string? Title
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Title));
            }
        } = title;

        public string? Description
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Description));
            }
        } = description;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AppendRealtime()
        {
            ToSql(Acceed.Shared.AgCl);

            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?._alerts!.Add(this);
        }

        public static Alert FromSql(OleDbDataReader reader)
        {
            var title = reader["AlertTitle"].ToString();
            var description = reader["AlertDescription"].ToString();
            var time = DateTime.Parse(reader["AlertTime"].ToString()!);

            var instance = new Alert(time, title, description);
            return instance;
        }

        public void ToSql(OleDbConnection connection)
        {
            var timeString = OccurTime.ToString("yyyy/M/dd HH:mm:ss");
            var sqlCommand = $"""
                INSERT INTO tb_Alert (AlertTitle, AlertDescription, AlertTime)
                VALUES (?, ?, ?)
                """;

            var command = new OleDbCommand(sqlCommand, connection);
            command.Parameters.AddWithValue("?", Title);
            command.Parameters.AddWithValue("?", Description);
            command.Parameters.AddWithValue("?", timeString);

            command.ExecuteNonQuery();
        }

        public void DeleteSql(OleDbConnection connection)
        {
            var sqlCommand = $"""
                DELETE FROM tb_Alert
                WHERE AlertTitle = ? and AlertDescription = ?
                """;

            var command = new OleDbCommand(sqlCommand, connection);
            command.Parameters.AddWithValue("?", Title);
            command.Parameters.AddWithValue("?", Description);

            command.ExecuteNonQuery();
        }
    }
}

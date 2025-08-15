using System.Data.OleDb;

namespace Citation.Model
{
    public class Alert(DateTime occurTime, string? title, string? description) : IDbContext<Alert>
    {
        public DateTime OccurTime { get; set; } = occurTime;
        public string? Title { get; set; } = title;

        public string? Description { get; set; } = description;

        public void AppendRealtime() 
        {
            var insertCommand = ToSql();
            Acceed.Shared.Execute(insertCommand);

            MainWindow.This._alerts.Add(this);
        }

        public static Alert FromSql(OleDbDataReader reader)
        {
            var title = reader["AlertTitle"].ToString();
            var description = reader["Description"].ToString();
            var time = DateTime.Parse(reader["AlertTime"].ToString()!);

            var instance = new Alert(time, title, description);
            return instance;
        }

        public string ToSql()
        {
            var timeString = OccurTime.ToString("yyyy/M/dd HH:mm:ss");
            var sqlCommand = $"""
                INSERT INTO tb_Alert (AlertTitle, AlertDescription, AlertTime)
                VALUES ('{Title}', '{Description}', '{timeString}')
                """;
            return sqlCommand;
        }

        public string DeleteSql()
        {
            var sqlCommand = $"""
                DELETE FROM tb_Alert
                WHERE AlertTitle = '{Title}' and AlertDescription = '{Description}'
                """;
            return sqlCommand;
        }
    }
}

using System.Data.OleDb;

namespace Citation.Model
{
    /// <summary>
    /// As this category is invariably the first to bear the brunt of restructuring,
    /// It is hereby awarded the title of "Standardized Management Model Type".
    /// </summary>
    public class Task : IDbContext<Task>
    {
        public Task(string name, string description, DateTime startTime, DateTime endTime, bool startRemind, bool endRemind)
        {
            Name = name;
            Description = description;
            StartTime = startTime;
            EndTime = endTime;
            StartRemind = startRemind;
            EndRemind = endRemind;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool StartRemind { get; set; }
        public bool EndRemind { get; set; }

        public void ToSql(OleDbConnection connection)
        {
            // I will definitely use Entity Framework next time I write a project mew~
            var startString = StartTime.ToString("yyyy/M/dd HH:mm:ss");
            var endString = EndTime.ToString("yyyy/M/dd HH:mm:ss");

            var sqlCommand = $"""
                INSERT INTO tb_Task (TaskName, TaskDescription,
                TaskStart, TaskEnd, TaskStartRemind, TaskEndRemind)
                VALUES (?, ?, ?, ?, ?, ?)
                """;

            var command = new OleDbCommand(sqlCommand, connection);

            command.Parameters.AddWithValue("?", Name);
            command.Parameters.AddWithValue("?", Description);
            command.Parameters.AddWithValue("?", startString);
            command.Parameters.AddWithValue("?", endString);
            command.Parameters.AddWithValue("?", StartRemind ? "Yes" : "No");
            command.Parameters.AddWithValue("?", EndRemind ? "Yes" : "No");

            command.ExecuteNonQuery();
        }

        public static Citation.Model.Task? FromSql(OleDbDataReader reader)
        {
            if (reader["TaskStart"].ToString() is null || reader["TaskEnd"].ToString() is null)
                return null;

            var name = reader["TaskName"].ToString();
            var description = reader["TaskDescription"].ToString();
            var startTime = DateTime.Parse(reader["TaskStart"].ToString()!);
            var endTime = DateTime.Parse(reader["TaskEnd"].ToString()!);
            var startRemind = (reader["TaskStartRemind"].ToString() == "Yes");
            var endRemind = (reader["TaskEndRemind"].ToString() == "Yes");

            // Ciallo～(∠・ω< )⌒★
            name ??= string.Empty;
            description ??= string.Empty;
            return new Citation.Model.Task(name, description, startTime, endTime,
                startRemind, endRemind);
        }
        // Damn, why is there a missing bracket here?

        public void DeleteSql(OleDbConnection connection)
        {
            var startString = StartTime.ToString("yyyy/M/dd HH:mm:ss");

            var sqlCommand = $"""
                DELETE FROM tb_Task
                WHERE TaskName = ?
                AND TaskStart = ?
                """;

            var command = new OleDbCommand(sqlCommand, connection);

            command.Parameters.AddWithValue("?", Name);
            command.Parameters.AddWithValue("?", startString);
            command.ExecuteNonQuery();
        }
    }
}

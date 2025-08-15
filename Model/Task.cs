using System.Data.OleDb;

namespace Citation.Model
{
    public class Task : IDbContext<Task>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool StartRemind { get; set; }
        public bool EndRemind { get; set; }

        public string ToSql()
        {
            // I will definitely use Entity Framework next time I write a project mew~
            var startString = StartTime.ToString("yyyy/M/dd HH:mm:ss");
            var endString = EndTime.ToString("yyyy/M/dd HH:mm:ss");
            var remindString = StartRemind ? "Yes" : "No";
            var alertString = EndRemind ? "Yes" : "No";

            var sqlCommand = $"""
                INSERT INTO tb_Task (TaskName, TaskDescription,
                TaskStart, TaskEnd, TaskStartRemind, TaskEndRemind)
                VALUES ('{Name}', '{Description}', '{startString}', 
                '{endString}', '{remindString}', '{alertString}')
                """;
            return sqlCommand;
        }

        public static Citation.Model.Task FromSql(OleDbDataReader reader)
        {
            var name = reader["TaskName"].ToString();
            var description = reader["TaskDescription"].ToString();
            var startTime = DateTime.Parse(reader["TaskStart"].ToString());
            var endTime = DateTime.Parse(reader["TaskEnd"].ToString());
            var startRemind = (reader["TaskStartRemind"].ToString() == "Yes");
            var endRemind = (reader["TaskEndRemind"].ToString() == "Yes");

            // Ciallo～(∠・ω< )⌒★
            return new Citation.Model.Task()
            {
                Name = name,
                Description = description,
                StartTime = startTime,
                EndTime = endTime,
                StartRemind = startRemind,
                EndRemind = endRemind
            };
        }
        // Damn, why is there a missing bracket here?
    }
}

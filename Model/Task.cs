using Citation.Utils;
using System.Data.OleDb;
using System.Windows;

namespace Citation.Model;

/// <summary>
/// As this category is invariably the first to bear the brunt of restructuring,
/// It is hereby awarded the title of "Standardized Management Model Type".
/// </summary>
public class Task : IDbContext<Task>
{
    public Task() { }

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

        var mainWindow = Application.Current.MainWindow as MainWindow;
        var password = mainWindow!.verify;
        startString = Cryptography.EncryptData(password, startString);
        endString = Cryptography.EncryptData(password, endString);

        var sqlCommand = $"""
            INSERT INTO tb_Task (TaskName, TaskDescription,
            TaskStart, TaskEnd, TaskStartRemind, TaskEndRemind)
            VALUES (?, ?, ?, ?, ?, ?)
            """;

        var command = new OleDbCommand(sqlCommand, connection);
        var encrypt = Cryptography.EncryptObject(password, this);

        command.Parameters.AddWithValue("?", encrypt.Name);
        command.Parameters.AddWithValue("?", encrypt.Description);
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

        var mainWindow = Application.Current.MainWindow as MainWindow;
        var password = mainWindow!.verify;

        var name = reader["TaskName"].ToString();
        var description = reader["TaskDescription"].ToString();

        var startTime = new DateTime(
            DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
            Random.Shared.Next(0, 12), Random.Shared.Next(0, 60),
            Random.Shared.Next(0, 60));
        var endTime = new DateTime(
            DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
            Random.Shared.Next(12, 24), Random.Shared.Next(0, 60),
            Random.Shared.Next(0, 60));

        try
        {
            startTime = DateTime.Parse(
               Cryptography.DecryptData(password, reader["TaskStart"].ToString()!));
            endTime = DateTime.Parse(
               Cryptography.DecryptData(password, reader["TaskEnd"].ToString()!));
        }
        catch (System.FormatException)
        {
            // ignored
        }

        var startRemind = (reader["TaskStartRemind"].ToString() == "Yes");
        var endRemind = (reader["TaskEndRemind"].ToString() == "Yes");

        // Ciallo～(∠・ω< )⌒★
        name ??= string.Empty;
        description ??= string.Empty;
        var model = new Citation.Model.Task(name, description, startTime, endTime,
            startRemind, endRemind);
        return Cryptography.DecryptObject(mainWindow.verify!, model);
    }

    public void DeleteSql(OleDbConnection connection)
    {
        var startString = StartTime.ToString("yyyy/M/dd HH:mm:ss");

        var sqlCommand = $"""
            DELETE FROM tb_Task
            WHERE TaskName = ?
            AND TaskStart = ?
            """;

        var command = new OleDbCommand(sqlCommand, connection);
        var mainWindow = Application.Current.MainWindow as MainWindow;
        var password = mainWindow!.verify;

        command.Parameters.AddWithValue("?", Cryptography.EncryptData(password, Name));
        command.Parameters.AddWithValue("?", Cryptography.EncryptData(password, startString));
        command.ExecuteNonQuery();
    }
}

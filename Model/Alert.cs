using Citation.Utils;
using System.ComponentModel;
using System.Data.OleDb;
using System.Windows;

namespace Citation.Model;

/// <summary>
/// Represents an alert entry containing occurrence time, title, and description information. Supports property change
/// notification and database persistence operations.
/// </summary>
/// <remarks>The Alert class is designed for use with data-binding scenarios and database operations. It
/// implements INotifyPropertyChanged to support UI updates when property values change. Instances can be created
/// manually or loaded from a database, and can be persisted or removed using the provided methods. Thread safety is not
/// guaranteed; access from multiple threads should be synchronized externally if required.</remarks>
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

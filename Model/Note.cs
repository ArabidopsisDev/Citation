using Citation.Utils;
using System.Data.OleDb;
using System.Windows;

namespace Citation.Model;

public class Note : IDbContext<Note>
{
    public Note() { }

    public Note(string title, string richText, DateTime createTime)
    {
        Title = title;
        RichText = richText;
        CreateTime = createTime;
    }

    public string Title { get; set; }

    public string RichText { get; set; }

    public DateTime CreateTime { get; set; }

    public void ToSql(OleDbConnection connection)
    {
        var createString = CreateTime.ToString("yyyy/M/dd HH:mm:ss");
        var mainWindow = Application.Current.MainWindow as MainWindow;
        var password = mainWindow!.verify;
        createString = Cryptography.EncryptData(password, createString);

        var sqlCommand = $"""
            INSERT INTO tb_Note (NoteTitle, NoteRichText, NoteTime)
            VALUES (?, ?, ?)
            """;
        var command = new OleDbCommand(sqlCommand, connection);
        var encrypt = Cryptography.EncryptObject(password, this);

        command.Parameters.AddWithValue("?", encrypt.Title);
        command.Parameters.AddWithValue("?", encrypt.RichText);
        command.Parameters.AddWithValue("?", createString);

        command.ExecuteNonQuery();
    }

    public static Note? FromSql(OleDbDataReader reader)
    {
        if (reader["NoteTime"].ToString() is null)
            return null;

        var mainWindow = Application.Current.MainWindow as MainWindow;
        var password = mainWindow!.verify;
        var title = reader["NoteTitle"].ToString();
        var richText = reader["NoteRichText"].ToString();

        var createTime = DateTime.Now;
        try
        {
            createTime = DateTime.Parse(
                Cryptography.DecryptData(password, reader["NoteTime"].ToString()!));
        }
        catch (System.FormatException)
        {
            // ignored
        }

        title ??= string.Empty;
        richText ??= string.Empty;
        var model = new Note(title, richText, createTime);
        return Cryptography.DecryptObject(mainWindow.verify!, model);
    }

    public void DeleteSql(OleDbConnection connection)
    {
        var createString = CreateTime.ToString("yyyy/M/dd HH:mm:ss");
        var sqlCommand = $"""
            DELETE FROM tb_Note
            WHERE NoteTitle = ?
            AND NoteTime = ?
            """;
        var command = new OleDbCommand(sqlCommand, connection);
        var mainWindow = Application.Current.MainWindow as MainWindow;
        var password = mainWindow!.verify;

        command.Parameters.AddWithValue("?", Cryptography.EncryptData(password, Title));
        command.Parameters.AddWithValue("?", Cryptography.EncryptData(password, createString));
        command.ExecuteNonQuery();
    }
}
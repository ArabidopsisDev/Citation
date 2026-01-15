using Citation.Utils;
using System.ComponentModel;
using System.Data.OleDb;
using System.Windows;

namespace Citation.Model;

public class Instrument : IDbContext<Instrument>, INotifyPropertyChanged
{
    public Instrument() { }

    public string? Name
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(Name));
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

    public string? Number
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(Number));
        }
    }

    public string? ResponsiblePerson
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(ResponsiblePerson));
        }
    }

    public string? Price
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(Price));
        }
    }

    public string? Model
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(Model));
        }
    }

    public DateTime PurchaseDate
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(PurchaseDate));
        }
    }

    public string? Company
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(Company));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public static Instrument FromSql(OleDbDataReader reader)
    {
        var mainWindow = Application.Current.MainWindow as MainWindow;

        var name = reader["InstrumentName"].ToString();
        var description = reader["InstrumentDescription"].ToString();
        var number = reader["InstrumentNumber"].ToString();
        var responsiblePerson = reader["InstrumentResponsiblePerson"].ToString();
        var price = reader["InstrumentPrice"].ToString();
        var model = reader["InstrumentModel"].ToString();
        var company = reader["InstrumentCompany"].ToString();

        var purchaseDate = DateTime.MaxValue;
        try
        {
            purchaseDate = DateTime.Parse(Cryptography.DecryptData(
               mainWindow!.verify,
               reader["InstrumentPurchaseDate"].ToString()!));
        }
        catch (FormatException)
        {
            // ignored
        }

        var instance = new Instrument
        {
            Name = name,
            Description = description,
            Number = number,
            ResponsiblePerson = responsiblePerson,
            Price = price,
            Model = model,
            PurchaseDate = purchaseDate,
            Company = company
        };

        return Cryptography.DecryptObject(mainWindow!.verify, instance);
    }

    public void ToSql(OleDbConnection connection)
    {
        var purchaseDateString = PurchaseDate.ToString("yyyy/MM/dd");

        var sqlCommand = $"""
            INSERT INTO tb_Instrument (InstrumentName, InstrumentDescription, InstrumentNumber, InstrumentResponsiblePerson, InstrumentPrice, InstrumentModel, InstrumentPurchaseDate, InstrumentCompany)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?)
            """;

        var mainWindow = Application.Current.MainWindow as MainWindow;
        var command = new OleDbCommand(sqlCommand, connection);

        var encrypt = Cryptography.EncryptObject(mainWindow!.verify, this);

        command.Parameters.AddWithValue("?", encrypt.Name);
        command.Parameters.AddWithValue("?", encrypt.Description);
        command.Parameters.AddWithValue("?", encrypt.Number);
        command.Parameters.AddWithValue("?", encrypt.ResponsiblePerson);
        command.Parameters.AddWithValue("?", encrypt.Price);
        command.Parameters.AddWithValue("?", encrypt.Model);
        command.Parameters.AddWithValue("?",
            Cryptography.EncryptData(mainWindow!.verify, purchaseDateString));
        command.Parameters.AddWithValue("?", encrypt.Company);

        command.ExecuteNonQuery();
    }

    public void DeleteSql(OleDbConnection connection)
    {
        var sqlCommand = $"""
            DELETE FROM tb_Instrument
            WHERE InstrumentNumber = ?
            """;

        var command = new OleDbCommand(sqlCommand, connection);
        var mainWindow = Application.Current.MainWindow as MainWindow;
        var password = mainWindow!.verify;

        command.Parameters.AddWithValue("?",
            Cryptography.EncryptData(password!, Number!));

        command.ExecuteNonQuery();
    }
}
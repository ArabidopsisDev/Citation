using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Citation.Model.Preserve;

/// <summary>
/// Represents the application configuration settings, including database credentials, security options, and API keys.
/// This class supports property change notification and XML serialization.
/// </summary>
/// <remarks>The Config class is designed for use as a data container for application settings that can be
/// serialized to or deserialized from XML. It implements INotifyPropertyChanged to support data binding scenarios, such
/// as in UI frameworks. Property changes will raise the PropertyChanged event to notify listeners. The class is marked
/// as serializable and can be saved to or loaded from an XML file using standard .NET serialization
/// mechanisms.</remarks>
[Serializable]
[XmlRoot("config")]
public class Config : INotifyPropertyChanged
{

    public string DbPassword
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(nameof(DbPassword));
            }
        }
    }

    public bool EnableSecurity
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(nameof(EnableSecurity));
            }
        }
    }

    public string SecurityVersion
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(nameof(SecurityVersion));
            }
        }
    }

    public string DeepSeekApiKey
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(nameof(DeepSeekApiKey));
            }
        }
    }

    public bool ReadLicense { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public void SaveToFile(string filePath = "config.xml")
    {
        var serializer = new XmlSerializer(typeof(Config));
        using (var writer = new StreamWriter(filePath))
        {
            serializer.Serialize(writer, this);
        }
    }
}
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Citation.Model.Preserve;

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
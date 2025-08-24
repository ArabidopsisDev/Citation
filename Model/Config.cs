using System.ComponentModel;
using System.Xml.Serialization;

namespace Citation.Model
{
    [Serializable]
    [XmlRoot("config")]
    public class Config : INotifyPropertyChanged
    {
        public string DbPassword
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(DbPassword));
            }
        }

        public bool EnableSecurity
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(EnableSecurity));
            }
        }

        public string SecurityVersion
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(SecurityVersion));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
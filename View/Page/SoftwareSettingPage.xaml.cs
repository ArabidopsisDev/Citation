using System.IO;
using System.Windows.Controls;
using System.Xml.Serialization;
using Citation.Model;

namespace Citation.View.Page
{
    /// <summary>
    /// Interaction logic for SoftwareSettingPage.xaml
    /// </summary>
    public partial class SoftwareSettingPage : UserControl
    {
        public Config Config { get; set; }

        public SoftwareSettingPage()
        {
            InitializeComponent();

            var serializer = new XmlSerializer(typeof(Config));
            using (var reader = new StreamReader("config.xml"))
                Config = (Config)serializer.Deserialize(reader)!;

            this.DataContext = this;
        }
    }
}
using Citation.Model.Preserve;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Config.SaveToFile("config.xml");

            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow!.ShowToast("设置已保存");
            mainWindow.Config = Config;
        }
    }
}
using Citation.Constant;
using System.Windows.Controls;

namespace Citation.View.Page
{
    /// <summary>
    /// Interaction logic for AboutPage.xaml
    /// </summary>
    public partial class AboutPage : UserControl
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            VersionLabel.Text = $"{Properties.Resources.AboutPage_VersionLeft}{AppInfo.AppVersion.ToString()}{Properties.Resources.AboutPage_VersionRight}";
        }
    }
}

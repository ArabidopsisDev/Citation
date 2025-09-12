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
            VersionLabel.Text = $"版本: v4（内部版本 Dev_{AppInfo.AppVersion.ToString()}）";
        }
    }
}

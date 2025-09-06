using Citation.Utils;
using System.IO;
using System.Windows;
using System.Windows.Documents;

namespace Citation.View.Statement
{
    public partial class LicenseWindow : Window
    {
        public LicenseWindow()
        {
            InitializeComponent();
            LoadLicenseContent();
        }

        private void LoadLicenseContent()
        {
            try
            {
                string rtfFilePath = "pack://application:,,,/Citation;component/Images/license.rtf";
                Uri uri = new Uri(rtfFilePath, UriKind.Absolute);

                using (Stream stream = Application.GetResourceStream(uri).Stream)
                {
                    TextRange range = new TextRange(LicenseTextBox.Document.ContentStart, LicenseTextBox.Document.ContentEnd);
                    range.Load(stream, DataFormats.Rtf);
                }
            }
            catch (Exception ex)
            {
                LogException.Collect(ex, LogException.ExceptionLevel.Error);
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow!.Config.ReadLicense = true;
            mainWindow.Config.SaveToFile();

            DialogResult = true;
            Close();
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
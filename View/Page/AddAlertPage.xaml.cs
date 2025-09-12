using Citation.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Citation.View.Page
{
    /// <summary>
    /// Interaction logic for AddAlertPage.xaml
    /// </summary>
    public partial class AddAlertPage : UserControl, INotifyPropertyChanged
    {
        // Oh my fuckness, it turns out that you only need to listen externally to achieve
        // two-way binding, and you don’t need to bind the internal properties
        public Alert? Alert
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Alert));
            }
        }

        public AddAlertPage()
        {
            InitializeComponent();

            Alert = new Model.Alert(DateTime.Now, "", "");
            DataContext = this;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Alert?.AppendRealtime();

            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow!.ShowToast("提醒添加成功");
            mainWindow.MainFrame.GoBack();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Alert = new Model.Alert(DateTime.Now, "", "");
        }
    }
}

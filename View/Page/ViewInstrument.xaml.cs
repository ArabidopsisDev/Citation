using Citation.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Citation.View.Page
{
    public partial class ViewInstrumentPage : UserControl
    {
        public ObservableCollection<Instrument> Instruments { get; set; } = new ObservableCollection<Instrument>();

        public ViewInstrumentPage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Instruments.Clear();
            var reader = Acceed.Shared.Query("SELECT * FROM tb_Instrument");
            while (reader.Read())
                Instruments.Add(Instrument.FromSql(reader));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Instrument instrument)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.NavigateWithSlideAnimation(new AddInstrumentPage(instrument));
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Instrument instru)
            {
                instru.DeleteSql(Acceed.Shared.Connection);
                Instruments.Remove(instru);

                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow!.ShowToast("删除仪器成功");
            }
        }
    }
}
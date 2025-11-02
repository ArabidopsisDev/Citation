using Citation.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Citation.View.Page
{
    /// <summary>
    /// AddInstrumentPage.xaml 的交互逻辑
    /// </summary>
    public partial class AddInstrumentPage : UserControl, INotifyPropertyChanged
    {
        private Instrument _instrument;
        private bool update = false;

        public Instrument Instrument
        {
            get => _instrument;
            set
            {
                _instrument = value;
                OnPropertyChanged(nameof(Instrument));
            }
        }

        public AddInstrumentPage(Instrument? instrument = null)
        {
            InitializeComponent();

            Instrument = new Instrument
            {
                Name = string.Empty,
                Description = string.Empty,
                Number = string.Empty,
                ResponsiblePerson = string.Empty,
                Price = string.Empty,
                Model = string.Empty,
                PurchaseDate = DateTime.Now,
                Company = string.Empty
            };

            if (instrument != null)
            {
                update = true;
                Instrument = instrument;
            }

            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Instrument.Name))
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.ShowToast(Properties.Resources.AddInstrumentPage_LackName);
                return;
            }

            if (string.IsNullOrEmpty(Instrument.Number))
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.ShowToast(Properties.Resources.AddInstrumentPage_LackNumber);
                return;
            }

            try
            {
                // Save to db
                if (update) Instrument.DeleteSql(Acceed.Shared.Connection);
                Instrument.ToSql(Acceed.Shared.Connection);

                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.ShowToast(Properties.Resources.AddInstrumentPage_Success);

                CancelButton_Click(this, null!);
            }
            catch (System.Exception ex)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.ShowToast($"{Properties.Resources.AddInstrumentPage_Failure}{ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Instrument = new Instrument
            {
                Name = string.Empty,
                Description = string.Empty,
                Number = string.Empty,
                ResponsiblePerson = string.Empty,
                Price = string.Empty,
                Model = string.Empty,
                PurchaseDate = DateTime.Now,
                Company = string.Empty
            };
        }
    }
}
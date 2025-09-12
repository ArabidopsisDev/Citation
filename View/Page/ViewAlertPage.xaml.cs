using Citation.Model;
using Citation.Utils;
using System.Windows;
using System.Windows.Controls;

namespace Citation.View.Page
{
    public partial class ViewAlertPage : UserControl
    {
        private MainWindow _mainWindow;
        private Alert? _currentAlert;
        private bool _isAddingNew = false;

        public ViewAlertPage()
        {
            InitializeComponent();
            Loaded += ViewAlertPage_Loaded;
        }

        private void ViewAlertPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Damn! I can actually write like this
            _mainWindow = Application.Current.MainWindow as MainWindow;
            DataContext = _mainWindow?.Alerts;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            _currentAlert = button?.Tag as Alert;

            if (_currentAlert != null)
            {
                _isAddingNew = false;
                EditTitle.Text = "编辑提醒";
                TitleTextBox.Text = _currentAlert.Title!;
                DescriptionTextBox.Text = _currentAlert.Description!;
                DatePicker.SelectedDate = _currentAlert.OccurTime.Date;
                TimePicker.Value = new DateTime(_currentAlert.OccurTime.TimeOfDay.Ticks);

                EditPanel.Visibility = Visibility.Visible;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button?.Tag is Alert alertToDelete)
            {
                try
                {
                    alertToDelete.DeleteSql(Acceed.Shared.Connection);
                    _mainWindow.Alerts!.Remove(alertToDelete);
                    _mainWindow.ShowToast("提醒已删除");
                }
                catch (Exception ex)
                {
                    _mainWindow.ShowToast("删除提醒失败");
                    LogException.Collect(ex, LogException.ExceptionLevel.Warning);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                _mainWindow.ShowToast("请输入提醒标题");
                return;
            }

            if (!DatePicker.SelectedDate.HasValue || !TimePicker.Value!.HasValue)
            {
                _mainWindow.ShowToast("请选择提醒时间");
                return;
            }

            var selectedDate = DatePicker.SelectedDate.Value;
            var selectedTime = TimePicker.Value!.Value;
            var occurTime = new DateTime(
                selectedDate.Year, selectedDate.Month, selectedDate.Day,
                selectedTime.Hour, selectedTime.Minute, selectedTime.Second);

            try
            {
                if (_isAddingNew)
                {
                    var newAlert = new Alert(
                        occurTime,
                        TitleTextBox.Text,
                        DescriptionTextBox.Text);

                    newAlert.ToSql(Acceed.Shared.Connection);
                    _mainWindow.Alerts!.Add(newAlert);
                }
                else if (_currentAlert != null)
                {
                    _currentAlert.DeleteSql(Acceed.Shared.Connection);

                    _currentAlert.Title = TitleTextBox.Text;
                    _currentAlert.Description = DescriptionTextBox.Text;
                    _currentAlert.OccurTime = occurTime;

                    _currentAlert.ToSql(Acceed.Shared.Connection);
                }

                _mainWindow.ShowToast("保存成功");
                EditPanel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                _mainWindow.ShowToast("保存失败");
                LogException.Collect(ex, LogException.ExceptionLevel.Warning);
            }
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            EditPanel.Visibility = Visibility.Collapsed;
        }
    }
}
using Citation.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Citation.View.Page
{
    /// <summary>
    /// AddTaskPage.xaml 的交互逻辑
    /// </summary>
    public partial class AddTaskPage : UserControl, INotifyPropertyChanged
    {
        public Model.Task? Task
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Task));
            }
        }

        public AddTaskPage()
        {
            InitializeComponent();

            Task = new Citation.Model.Task(string.Empty, string.Empty,
                DateTime.Now, DateTime.Now, false, false);
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Task.ToSql(Acceed.Shared.Connection);

            if (Task.StartRemind)
                new Alert(Task.StartTime, $"{Task.Name}开始了", Task.Description).AppendRealtime();
            if (Task.EndRemind)
                new Alert(Task.EndTime, $"{Task.Name}结束了", Task.Description).AppendRealtime();

            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.ShowToast("任务添加成功");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Task = new Citation.Model.Task(string.Empty, string.Empty,
                DateTime.Now, DateTime.Now, false, false);
        }
    }
}

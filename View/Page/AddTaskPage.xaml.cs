using Citation.Model;
using System.Windows;
using System.Windows.Controls;

namespace Citation.View.Page
{
    /// <summary>
    /// AddTaskPage.xaml 的交互逻辑
    /// </summary>
    public partial class AddTaskPage : UserControl
    {
        public Citation.Model.Task Task { get; set; } = new Citation.Model.Task(
            string.Empty,string.Empty, DateTime.Now, DateTime.Now, false, false);

        public AddTaskPage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Task.ToSql(Acceed.Shared.Connection);

            if (Task.StartRemind)
                new Alert(Task.StartTime, Task.Name, Task.Description).AppendRealtime();
            if (Task.EndRemind)
                new Alert(Task.EndTime, Task.Name, Task.Description).AppendRealtime();

            MainWindow.This.ShowToast("任务添加成功");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.This.MainFrame.GoBack();
        }
    }
}

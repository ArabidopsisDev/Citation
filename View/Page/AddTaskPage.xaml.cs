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
        public Citation.Model.Task Task { get; set; } = new Citation.Model.Task();

        public AddTaskPage()
        {
            InitializeComponent();

            this.Task.StartTime = DateTime.Now;
            this.Task.EndTime = DateTime.Now;
            this.DataContext = this;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var insertCommand = this.Task.ToSql();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.This.MainFrame.GoBack();
        }
    }
}

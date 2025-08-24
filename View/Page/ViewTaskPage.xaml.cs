using Citation.Model;
using Citation.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Citation;

namespace Citation.View.Page
{
    public partial class ViewTaskPage : UserControl
    {
        private Citation.Model.Task _selectedTask;

        public ViewTaskPage(List<Citation.Model.Task> todayTasks)
        {
            InitializeComponent();
            TodayTasks = todayTasks;
        }

        private void RenderTasks()
        {
            ScheduleCanvas.Children.Clear();

            foreach (var task in TodayTasks)
            {
                var top = task.StartTime.Hour * 100 + task.StartTime.Minute * 100 / 60.0;
                var height = (task.EndTime - task.StartTime).TotalHours * 100;

                var color = GetTaskColor(task.Name);

                var taskBlock = new Border
                {
                    Width = ScheduleCanvas.ActualWidth - 5,
                    Height = height,
                    Background = new SolidColorBrush(color),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(3),
                    Tag = task
                };

                taskBlock.MouseDown += TaskBlock_MouseDown;

                taskBlock.MouseEnter += (s, e) =>
                {
                    taskBlock.Background = new SolidColorBrush(color.AdjustBrightness(0.9));
                    taskBlock.Cursor = Cursors.Hand;
                };

                taskBlock.MouseLeave += (s, e) =>
                {
                    taskBlock.Background = new SolidColorBrush(color);
                    taskBlock.Cursor = Cursors.Arrow;
                };

                Canvas.SetTop(taskBlock, top);
                Canvas.SetLeft(taskBlock, 2.5);

                var textBlock = new TextBlock
                {
                    FontFamily = new FontFamily("Calibri"),
                    Text = $"{task.Name}\n{task.StartTime:HH:mm}-{task.EndTime:HH:mm}",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = GetContrastColor(color),
                    FontSize = 16,
                    Margin = new Thickness(5)
                };

                taskBlock.Child = textBlock;
                ScheduleCanvas.Children.Add(taskBlock);
            }
        }

        private void TaskBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border?.Tag is Citation.Model.Task task)
            {
                _selectedTask = task;
                ShowTaskDetails(task);
            }
        }

        private void ShowTaskDetails(Citation.Model.Task task)
        {
            DetailName.Text = task.Name;
            DetailDescription.Text = task.Description;
            DetailStartTime.Text = $"开始: {task.StartTime:yyyy-MM-dd HH:mm}";
            DetailEndTime.Text = $"结束: {task.EndTime:yyyy-MM-dd HH:mm}";
            DetailStartRemind.IsChecked = task.StartRemind;
            DetailEndRemind.IsChecked = task.EndRemind;

            TaskDetailPanel.Visibility = Visibility.Visible;
        }

        private void CloseDetail_Click(object sender, RoutedEventArgs e)
        {
            TaskDetailPanel.Visibility = Visibility.Collapsed;
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTask == null) return;

            try
            {
                using (var connection = Acceed.Shared.Connection)
                {
                    if (connection.State == System.Data.ConnectionState.Closed)
                    {
                        connection.Open();
                    }

                    _selectedTask.DeleteSql(connection);
                }

                // Remove alert
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (_selectedTask.StartRemind)
                    mainWindow!.RemoveAlert(_selectedTask.Name, _selectedTask.StartTime);
                if (_selectedTask.EndRemind)
                    mainWindow!.RemoveAlert(_selectedTask.Name, _selectedTask.EndTime);

                // Remove visual
                TodayTasks.Remove(_selectedTask);
                RenderTasks();

                TaskDetailPanel.Visibility = Visibility.Collapsed;
                mainWindow?.ShowToast("任务已成功删除");
            }
            catch (Exception ex)
            {
                LogException.Collect(ex, LogException.ExceptionLevel.Warning);

                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.ShowToast("未能删除任务");
            }
        }


        public List<Citation.Model.Task> TodayTasks { get; set; }
        public List<TimeLabel> TimeLabels { get; } = [];

        private readonly System.Windows.Threading.DispatcherTimer _indicateTimer = new();
        private DateTime _lastUpdateTime;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            GenerateTimeLabels();
            RenderTasks();

            _indicateTimer.Interval = TimeSpan.FromSeconds(15);
            _indicateTimer.Tick += (_, _) => UpdateCurrentTimeIndicator();
            _indicateTimer.Start();

            var displayTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            displayTimer.Tick += (_, _) =>
            {
                var now = DateTime.Now;
                _lastUpdateTime = now;

                CurrentTimeText.Text = now.ToString("HH:mm:ss");
            };
            displayTimer.Start();

            UpdateCurrentTimeIndicator();
        }

        private void GenerateTimeLabels()
        {
            for (var hour = 0; hour < 24; hour++)
            {
                TimeLabels.Add(new TimeLabel
                {
                    Time = $"{hour:00}:00",
                    Height = 100
                });
            }

            DataContext = this;
        }

        private static Color GetTaskColor(string taskName)
        {
            var hash = taskName.GetHashCode();
            return Color.FromRgb(
                (byte)((hash & 0xFF0000) >> 16),
                (byte)((hash & 0x00FF00) >> 8),
                (byte)(hash & 0x0000FF)
            ).AdjustBrightness(0.7);
        }

        private static SolidColorBrush GetContrastColor(Color color)
        {
            var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
            return luminance > 0.5 ? Brushes.Black : Brushes.White;
        }

        private void UpdateCurrentTimeIndicator()
        {
            var now = DateTime.Now;

            var minutesFromMidnight = now.TimeOfDay.TotalMinutes;
            var currentPosition = minutesFromMidnight * 100 / 60;

            Canvas.SetTop(CurrentTimeLine, currentPosition);

            var scrollAnimation = new DoubleAnimation
            {
                To = currentPosition - (MainScrollViewer.ViewportHeight / 2),
                Duration = TimeSpan.FromSeconds(0.5),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            MainScrollViewer.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, scrollAnimation);
        }
    }
}
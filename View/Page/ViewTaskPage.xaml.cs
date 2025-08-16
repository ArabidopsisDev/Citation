using Citation.Model;
using Citation.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Citation.View.Page
{
    public partial class ViewTaskPage : UserControl
    {
        // When duplication occurs, it is more polite to write the fully qualified name
        public List<Citation.Model.Task> TodayTasks { get; set; }
        public List<TimeLabel> TimeLabels { get; } = [];

        private readonly System.Windows.Threading.DispatcherTimer _indicateTimer = new();
        private DateTime _lastUpdateTime;

        public ViewTaskPage(List<Citation.Model.Task> todayTasks)
        {
            InitializeComponent();

            TodayTasks = todayTasks;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            GenerateTimeLabels();
            RenderTasks();

            // Big and small core technology (not)
            _indicateTimer.Interval = TimeSpan.FromSeconds(15);
            _indicateTimer.Tick += (_, _) => UpdateCurrentTimeIndicator();
            _indicateTimer.Start();

            // So strong? Just how strong?
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

            // Not the data I like, bind directly
            DataContext = this;
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
                    CornerRadius = new CornerRadius(3)
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
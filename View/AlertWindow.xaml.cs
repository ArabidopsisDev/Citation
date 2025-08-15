using Citation.Model;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows;
using System.Media;

namespace Citation.View
{
    public partial class AlertWindow : Window
    {
        public Alert Alert { get; set; }

        public AlertWindow(Alert alert)
        {
            InitializeComponent();
            Alert = alert;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            DateTime dateTime = Alert.OccurTime;

            txtTitle.Text = Alert.Title;
            txtDescription.Text = Alert.Description;
            txtTime.Text = $"当前时间: {dateTime:yyyy-MM-dd HH:mm:ss}";

            Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
            BeginAnimation(OpacityProperty, fadeIn);

            var stream = Application.GetResourceStream(
               new Uri("pack://application:,,,/Citation;component/Images/misc.wav")).Stream;

            var player = new SoundPlayer(stream);
            player.Play();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.2));
            fadeOut.Completed += (s, _) => Close();
            BeginAnimation(OpacityProperty, fadeOut);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }
    }
}
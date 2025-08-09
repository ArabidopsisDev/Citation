using Citation.View;
using System.Windows;
using Citation.Model;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace Citation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Project Project { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Project ??= new Project
            {
                Name = "尚未打开项目！",
                Authors = new ObservableCollection<string>(),
                Guid = System.Guid.NewGuid().ToString()
            };

            DataContext = Project;
        }

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            var newProjectWindow = new ProjectEditWindow();
            newProjectWindow.Show();
        }

        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".accdb",
                Filter = "Citation project (.accdb)|*.accdb|All files (*.*)|*.*"
            };

            var result = dialog.ShowDialog();
            if (result == true)
            {
                var filename = dialog.FileName;

                // Verify file type
                List<int> fileHead = [0x00, 0x01, 0x00, 0x00, 0x53, 0x74, 0x61, 0x6E,
                    0x64, 0x61, 0x72, 0x64, 0x20, 0x41, 0x43, 0x45, 0x20, 0x44, 0x42];
                using var fs = new FileStream(filename, FileMode.Open);
                var binaryReader = new BinaryReader(fs);

                if (fileHead.Any(bit => binaryReader.ReadChar() != (char)bit))
                {
                    System.Windows.MessageBox.Show("未知项目类型或项目损坏", "打开失败",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                fs.Close();

                // Load info from database
                Acceed.Shared.ReConnect(filename);
                var reader = Acceed.Shared.Query("SELECT * FROM tb_Basic");
                while (reader.Read())
                {
                    Project.Name = reader["ProjectName"].ToString();
                    Project.Path = reader["ProjectPath"].ToString();
                    Project.Guid = reader["ProjectGuid"].ToString();

                    var authors = reader["ProjectAuthors"].ToString()!.Split('/');
                    Project.Authors = [..authors];
                }
            }
        }

        private void NavigateWithSlideAnimation(UserControl page)
        {
            if (Project.Name == "尚未打开项目！")
            {
                System.Windows.MessageBox.Show("请先打开一个项目", "操作失败",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var storyboard = new Storyboard();
            var fadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity"));

            var slideAnimation = new DoubleAnimation
            {
                From = 100,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("RenderTransform.X"));

            page.RenderTransform = new TranslateTransform();
            page.Opacity = 0;

            storyboard.Children.Add(fadeInAnimation);
            storyboard.Children.Add(slideAnimation);

            MainFrame.Navigated += (sender, e) =>
            {
                storyboard.Begin(page);
            };

            MainFrame.Navigate(page);
        }

        private void AddCitation_Click(object sender, RoutedEventArgs e)
        {
            NavigateWithSlideAnimation(new Citation.View.Page.CitationPage());
        }
    }
}
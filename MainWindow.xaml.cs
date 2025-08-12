using Citation.View;
using System.Windows;
using Citation.Model;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Threading;
using Citation.Model.Reference;
using System.Text.Json;

namespace Citation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Project Project { get; set; }
        internal static MainWindow This;

        public MainWindow()
        {
            InitializeComponent();
            This = this;

            Project ??= new Project
            {
                Name = "尚未打开项目！",
                Authors = new ObservableCollection<string>(),
                Guid = System.Guid.NewGuid().ToString()
            };

            NavigateWithSlideAnimation(new Citation.View.Page.WelcomePage(), false);
            DataContext = Project;
        }

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            var newProjectWindow = new ProjectEditWindow(null, LoadProject);
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
                LoadProject(dialog.FileName);
            MainFrame.Content = null;
        }

        private void LoadProject(string dbPath, bool check = true)
        {
            var filename = dbPath;

            if (check)
            {
                // Verify file type
                List<int> fileHead = [0x00, 0x01, 0x00, 0x00, 0x53, 0x74, 0x61, 0x6E,
                0x64, 0x61, 0x72, 0x64, 0x20, 0x41, 0x43, 0x45, 0x20, 0x44, 0x42];
                using var fs = new FileStream(filename, FileMode.Open);
                var binaryReader = new BinaryReader(fs);

                if (fileHead.Any(bit => binaryReader.ReadChar() != (char)bit))
                {
                    ShowToast("打开失败：未知项目类型或项目损坏");
                    return;
                }
                fs.Close();
            }

            // Load info from database
            Acceed.Shared.ReConnect(filename);
            var reader = Acceed.Shared.Query("SELECT * FROM tb_Basic");
            while (reader.Read())
            {
                Project.Name = reader["ProjectName"].ToString();
                Project.Path = reader["ProjectPath"].ToString();
                Project.Guid = reader["ProjectGuid"].ToString();

                var authors = reader["ProjectAuthors"].ToString()!.Split('/');
                Project.Authors = [.. authors];
            }
        }

        internal void NavigateWithSlideAnimation(UserControl page, bool project = true)
        {
            if (project && Project.Name == "尚未打开项目！")
            {
                ShowToast("清先打开一个项目");
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

        public void ShowToast(string message)
        {
            ToastContainer.Children.Clear();

            var border = new Border
            {
                Style = (Style)FindResource("BubbleToastStyle"),
                Child = new TextBlock
                {
                    Text = message,
                    Foreground = Brushes.White,
                    FontSize = 14,
                    FontWeight = FontWeights.Medium
                }
            };

            ToastContainer.Children.Add(border);

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4.7) };
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                ToastContainer.Children.Remove(border);
            };
            timer.Start();
        }

        private void AddCitation_Click(object sender, RoutedEventArgs e)
        {
            NavigateWithSlideAnimation(new Citation.View.Page.CitationPage());
        }

        private void ViewCitation_Click(object sender, RoutedEventArgs e)
        {
            NavigateWithSlideAnimation(new Citation.View.Page.ViewReferencePage());
        }

        private void ExportCitation_Click(object sender, RoutedEventArgs e)
        {
            NavigateWithSlideAnimation(new Citation.View.Page.ViewReferencePage());
        }

        private async void ImportCitation_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".ris",
                Filter = "参考文献文件 (.ris)|*.ris"
            };

            var result = dialog.ShowDialog();
            if (result == true)
            {
                var fileName = dialog.FileName;

                using var reader = new StreamReader(fileName);
                var text = await reader.ReadToEndAsync();
                var cites = text.Split('\n');
                var dois = new List<string>();

                foreach (var cite in cites)
                {
                    if (string.IsNullOrEmpty(cite)) continue;
                    var info = cite.Split('\n');
                    var doi = "";

                    foreach (var s in info)
                        if (s.StartsWith("UR"))
                            doi = s.Split('-')[1].Trim();
                    if (!string.IsNullOrEmpty(doi)) dois.Add(doi);
                }

                ShowToast("开始获取文献，时间较长，请坐和放宽");

                // Still an asynchronous master
                var httpClient = new HttpClient();
                foreach (var doi in dois)
                {
                    try
                    {
                        using var response = await httpClient.GetAsync($"https://api.crossref.org/works/{doi}");
                        response.EnsureSuccessStatusCode();
                        var source = await response.Content.ReadAsStringAsync();

                        var journalArticle = JsonSerializer.Deserialize<JournalArticle>(source);
                        journalArticle.Message.AfterWards();

                        var insertCommand = journalArticle.ToSql();
                        Acceed.Shared.Execute(insertCommand);
                        ShowToast($"[{journalArticle!.Message!.Title![0]}] 获取成功");
                    }
                    catch (Exception ex)
                    {
                        ShowToast($"[{doi}] 获取失败");
                    }
                }
                ShowToast($"导入结束");
            }
        }

        private void ExitSoftware_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
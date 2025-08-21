using Citation.Model;
using Citation.Model.Reference;
using Citation.Utils;
using Citation.View;
using Citation.View.Page;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Task = Citation.Model.Task;

namespace Citation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        internal bool Limited = false;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Project Project
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Project));
            }
        }

        // Di su zhi
        internal static MainWindow This;

        internal List<Alert>? _alerts = [];

        public MainWindow()
        {
            InitializeComponent();

            Project ??= new Project
            {
                Name = "尚未打开项目！",
                Authors = [],
                Guid = System.Guid.NewGuid().ToString()
            };

            MainWindow.This = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NavigateWithSlideAnimation(new Citation.View.Page.WelcomePage(), false);
            DataContext = Project;

            MainFrame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
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
                Project.Password = reader["ProjectPassword"].ToString();
                var authors = reader["ProjectAuthors"].ToString()!.Split('/');
                Project.Authors = [.. authors];
            }

            if (!string.IsNullOrEmpty(Project.Password))
            {
                // Start verify process
                var authWindow = new VerifyWindow(Authorize);
                var result = authWindow.ShowDialog();

                if (result == true)
                {
                    ShowToast("授权验证成功");
                }
                else
                {
                    ShowToast("授权失败或已取消");

                    this.Project = new Project
                    {
                        Name = "尚未打开项目！",
                        Authors = [],
                        Guid = ""
                    };
                    return;
                }
            }

            // Load alerts
            reader = Acceed.Shared.Query("SELECT * FROM tb_Alert");
            _alerts = [];

            while (reader.Read())
                _alerts.Add(Alert.FromSql(reader));

            // Set scheduled reminders
            var timer = new System.Timers.Timer(TimeSpan.FromSeconds(1));
            timer.Elapsed += (_, _) =>
            {
                if (_alerts is null) return;

                for (int i = 0; i< _alerts.Count; i++)
                {
                    Alert? item = _alerts[i];
                    if (item.OccurTime <= DateTime.Now)
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            var alert = new AlertWindow(item);
                            alert.Show();

                            item.DeleteSql(Acceed.Shared.Connection);
                        });
                        _alerts.Remove(item);
                    }
                }
            };

            timer.Start();
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

            var stream = Application.GetResourceStream(
                new Uri("pack://application:,,,/Citation;component/Images/alert.wav"))
                ?.Stream;

            SoundPlayer player = new(stream);
            player.Play();

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
            if (Project.Name == "尚未打开项目！")
            {
                ShowToast("清先打开一个项目");
                return;
            }

            var exportPage = new ExportPage();

            var reader = Acceed.Shared.Query("SELECT * FROM tb_Paper");
            var papers = new List<JournalArticle>();

            while (reader.Read())
            {
                var db = new JournalArticleDb()
                {
                    Abstract = reader["PaperAbstract"].ToString()!,
                    AuthorString = reader["PaperAuthor"].ToString()!,
                    Issue = reader["PaperIssue"].ToString()!,
                    ContainerString = reader["PaperContainer"].ToString()!,
                    Doi = reader["PaperDoi"].ToString()!,
                    Page = reader["PaperPage"].ToString()!,
                    TitleString = reader["PaperTitle"].ToString()!,
                    Volume = reader["PaperVolume"].ToString()!,
                    Link = reader["PaperLink"].ToString()!,
                    Url = reader["PaperUrl"].ToString()!,
                    Published = reader["PaperPublished"].ToString()!,
                    Folder = reader["PaperFolder"].ToString()!
                };

                db.Afterward();
                var paper = JournalArticle.FromArticle(db);

                paper.Message!.AfterWards();
                papers.Add(paper);
            }

            exportPage.ArticlesContainer.ItemsSource = papers;
            NavigateWithSlideAnimation(exportPage);
        }
        private async void ImportCitation_Click(object sender, RoutedEventArgs e)
        {
            if (Project.Name == "尚未打开项目！")
            {
                ShowToast("清先打开一个项目");
                return;
            }

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
                        if (journalArticle is null ||journalArticle.Message is null)
                        {
                            ShowToast($"[{doi}] 获取失败");
                            continue;
                        }

                        journalArticle.Message.AfterWards();

                        var insertCommand = journalArticle.ToSql();
                        Acceed.Shared.Execute(insertCommand);
                        ShowToast($"[{journalArticle!.Message!.Title![0]}] 获取成功");
                    }
                    catch (Exception ex)
                    {
                        LogException.Collect(ex, LogException.ExceptionLevel.Warning);
                        ShowToast($"[{doi}] 获取失败");
                    }
                }
                ShowToast($"导入结束");
            }
        }

        private void CloseProject_Click(object sender, RoutedEventArgs e)
        {
            if (Project.Name == "尚未打开项目！")
            {
                ShowToast("您还未打开项目，无需关闭");
                return;
            }

            // Clear back entry
            while (MainFrame.CanGoForward)
                MainFrame.GoForward();
            MainFrame.RemoveBackEntry();

            // Clear project buffer
            Acceed.Shared.Close();
            _alerts = null;
            Project = new Project
            {
                Name = "尚未打开项目！",
                Authors = [],
                Guid = ""
            };

            // Show for user
            NavigateWithSlideAnimation(new Citation.View.Page.WelcomePage(), false);
            ShowToast("项目已关闭");
            DataContext = Project;
        }

        private void ExitSoftware_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            NavigateWithSlideAnimation(new Citation.View.Page.AddTaskPage());
        }

        private void ViewTask_Click(object sender, RoutedEventArgs e)
        {
            if (Project.Name == "尚未打开项目！")
            {
                ShowToast("清先打开一个项目");
                return;
            }

            // Find today's tasks
            var reader = Acceed.Shared.Query("SELECT * FROM tb_Task");
            var tasks = new List<Citation.Model.Task>();
            while (reader.Read())
                tasks.Add(Task.FromSql(reader)!);

            for (int i = 0; i < tasks.Count; i++)
            {
                var now = DateTime.Now;
                var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                var todayEnd = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);

                // Normalization
                tasks[i].EndTime = tasks[i].EndTime > todayEnd ? todayEnd : tasks[i].EndTime;
                tasks[i].StartTime = tasks[i].StartTime < todayStart ? todayStart : tasks[i].StartTime;

                if (tasks[i].StartTime > todayEnd || tasks[i].EndTime < todayStart)
                    tasks.RemoveAt(i);
            }

            var viewPage = new ViewTaskPage(tasks);
            NavigateWithSlideAnimation(viewPage);
        }

        private void ImportFailure_Click(object sender, RoutedEventArgs e)
        {
            NavigateWithSlideAnimation(new ImportFailurePage(), false);
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            NavigateWithSlideAnimation(new SettingPage());
        }

        private void AddAlert_Click(object sender, RoutedEventArgs e)
        {
            NavigateWithSlideAnimation(new AddAlertPage());
        }

        internal void RemoveAlert(string title, DateTime alertTime)
        {
            for (int i = 0; i < _alerts.Count; i++)
            {
                if (_alerts[i].Title == title && _alerts[i].OccurTime == alertTime)
                {
                    _alerts[i].DeleteSql(Acceed.Shared.Connection);
                    _alerts.RemoveAt(i);
                }
            }
        }

        private void AuthorizeProject_Click(object sender, RoutedEventArgs e)
        {
            if (Limited)
            {
                ShowToast("通过文件授权的项目不能够创建授权文件");
                return;
            }

            if (Project.Name == "尚未打开项目！")
            {
                ShowToast("清先打开一个项目");
                return;
            }

            var authorizationWindow = new AuthorizationWindow();
            authorizationWindow.Show();
        }

        private bool Authorize(string passwordOrFile, bool usePassword)
        {
            if (usePassword)
            {
                var byteArray = Encoding.UTF8.GetBytes(passwordOrFile);
                Limited = false;
                return Verify.ConfirmByPassword(Convert.ToBase64String(byteArray));
            }

            Limited = true;
            return Verify.ConfirmByFile(passwordOrFile);
        }

        private void GenerateSerial_Click(object sender, RoutedEventArgs e)
        {
            var serial = new HardIdentifier().ToString();
            Clipboard.SetText(serial);
            ShowToast("已复制到剪切板");
        }
    }
}
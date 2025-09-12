using Citation.Model;
using Citation.Model.Reference;
using Citation.Utils;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Citation.View.Page
{
    public partial class ViewTimelinePage : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<JournalArticle>? Articles
        {
            get => field;
            set
            {
                field = value;
                ProcessArticles();
                OnPropertyChanged(nameof(Articles));
            }
        }

        public List<TimelineGroup> TimelineGroups
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(TimelineGroups));
            }
        } = new();

        public Dictionary<string, Brush> FolderColors
        {
            get => field;
            set
            {
                field = value;
                OnPropertyChanged(nameof(FolderColors));
            }
        } = new();

        public ViewTimelinePage()
        {
            InitializeComponent();
            TimelineGroups = new List<TimelineGroup>();
            DataContext = this;
        }

        public ViewTimelinePage(List<JournalArticle> articles) : this()
        {
            Articles = articles;
        }

        private void ProcessArticles()
        {
            if (Articles == null) return;

            var groups = new Dictionary<(int year, int month), TimelineGroup>();

            foreach (var article in Articles)
            {
                if (article.Message == null ||
                    string.IsNullOrEmpty(article.Message!.Title![0]) ||
                    article.Message.Published?.DateParts == null)
                    continue;

                int year = article.Message.Published.DateParts[0][0];
                int month = article.Message.Published.DateParts.Length > 1 ?
                    article.Message.Published.DateParts[1][0] : 1;

                Brush colorBrush = new SolidColorBrush(Color.FromRgb(178, 217, 189));
                if (!string.IsNullOrEmpty(article.Message.Folder))
                {
                    if (FolderColors.TryGetValue(article.Message.Folder, out var value))
                        colorBrush = value;
                    else
                        colorBrush = FolderColors[article.Message.Folder] = Randomization.GenerateLightBrush();
                }

                var item = new TimelineItem
                {
                    Article = article,
                    Year = year,
                    Month = month,
                    Color = colorBrush,
                    Conclusion = article.Message!.Title![0],
                    Title = article.Message.Title?.FirstOrDefault() ?? "Untitled",
                    Folder = article.Message.Folder ?? ""
                };

                var key = (year, month);
                if (!groups.ContainsKey(key))
                {
                    groups[key] = new TimelineGroup
                    {
                        Year = year,
                        Month = month,
                        Items = new List<TimelineItem>()
                    };
                }
                groups[key].Items.Add(item);
            }

            TimelineGroups = groups.Values.OrderBy(g => g.Year).ThenBy(g => g.Month).ToList();
        }

        private void TimelineScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer != null)
            {
                if (e.Delta > 0)
                    scrollViewer.LineLeft();
                else
                    scrollViewer.LineRight();
                e.Handled = true;
            }
        }

        private void TimelineItem_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is TimelineItem item)
            {
                System.Windows.MessageBox.Show($"标题: {item.Title}\n年份: {item.Year}\n月份: {item.Month}\n结论: {item.Conclusion}",
                    "论文详情", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
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
                    Folder = reader["PaperFolder"].ToString()!,
                };

                db.Afterward();
                papers.Add(JournalArticle.FromArticle(db));
            }

            Articles = papers;
        }
    }
}
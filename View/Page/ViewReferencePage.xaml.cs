using Citation.Model;
using Citation.Model.Reference;
using Citation.Utils;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Citation.View.Page
{
    public partial class ViewReferencePage : UserControl, INotifyPropertyChanged
    {
        public List<JournalArticle>? Articles { get; set; }

        private IFormatter? _formatter;

        public JournalArticle? SelectedArticle
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(SelectedArticle));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ViewReferencePage()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is JournalArticle article)
            {
                SelectedArticle = article;

                if (_formatter is null)
                {
                    LogException.Collect(new ArgumentException("指定格式化器不存在"), 
                        LogException.ExceptionLevel.Warning);
                    return;
                }

                var type = _formatter.GetType();
                _formatter = (IFormatter)Activator.CreateInstance(type, [SelectedArticle])!;
                CitationBox.Text = _formatter.ToMarkdown();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void ViewReferencePage_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Load reference & folder information
            var reader = Acceed.Shared.Query("SELECT * FROM tb_Paper");
            var papers = new List<JournalArticle>();
            var folders = new List<FolderItem>();

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
                var folder = folders.FirstOrDefault(item => item.Name == paper.Message!.Folder);

                if (folder is null)
                {
                    var newFolder = new FolderItem()
                    {
                        Name = paper.Message!.Folder,
                        Articles = [paper]
                    };

                    folders.Add(newFolder);
                }
                else
                {
                    folder.Articles.Add(paper);
                }
            }

            Articles = papers;
            FolderTreeView.ItemsSource = folders;

            // Load specific formatter
            var freader = Acceed.Shared.Query("SELECT * FROM tb_Setting");
            string formatter = "";
            while (freader.Read())
                formatter = freader["Formatter"].ToString()!;

            var targetNamespace = "Citation.Model.Format";
            var assembly = Assembly.GetExecutingAssembly();
            var formatters = assembly.GetTypes()
            .Where(t => string.Equals(t.Namespace, targetNamespace, StringComparison.Ordinal)
                && t.IsClass
                && !t.IsAbstract
                && typeof(IFormatter).IsAssignableFrom(t))
            .Select(t => (IFormatter)Activator.CreateInstance(t)!)
            .ToList();

            foreach (var item in formatters)
            {
                if (item.FormatName == formatter)
                {
                    _formatter = item;
                    break;
                }
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CitationBox.Text);
            MainWindow.This.ShowToast("引用已经复制到剪切板");
        }

        private void LatexButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedArticle is null) return;
            CitationBox.Text = _formatter?.ToLatex();
        }

        private void MarkdownButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedArticle is null) return;
            CitationBox.Text = _formatter?.ToMarkdown();
        }
    }
}
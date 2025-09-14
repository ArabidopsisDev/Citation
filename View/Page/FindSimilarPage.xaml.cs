using Citation.Model.Reference;
using Citation.Utils;
using Citation.Utils.Api;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Citation.View.Page
{
    public partial class FindSimilarPage : UserControl
    {
        private List<JournalArticle> _articles = new List<JournalArticle>();
        private List<JournalArticle> _existArticles = new List<JournalArticle>();

        public FindSimilarPage(List<JournalArticle> existArticles)
        {
            InitializeComponent();

            _existArticles = existArticles;
            SearchTextBox.Text = $"已自动加载{existArticles.Count}篇文章";
        }

        private void ClearArticles()
        {
            _articles.Clear();
            ResultsItemsControl.ItemsSource = null;
            UpdateResultsCount();
            NoResultsText.Visibility = Visibility.Collapsed;
        }

        private void AddArticles(List<JournalArticle> articles)
        {
            _articles.AddRange(articles);
            RefreshArticlesDisplay();
        }

        private void RefreshArticlesDisplay()
        {
            ResultsItemsControl.ItemsSource = null;
            ResultsItemsControl.ItemsSource = _articles;
            UpdateResultsCount();
        }

        private void UpdateResultsCount()
        {
            ResultsCountText.Text = $"找到 {_articles.Count} 篇相似文献";
        }


        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button != null)
            {
                var article = button.DataContext as JournalArticle;
                if (article != null)
                {
                    ViewArticleDetails(article);
                }
            }
        }

        private void DownloadPdfButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button != null)
            {
                var article = button.DataContext as JournalArticle;
                if (article != null)
                {
                    DownloadArticlePdf(article);
                }
            }
        }

        private async void OnSearchSimilarArticles(object sender, RoutedEventArgs e)
        {
            // build prompt
            var promptBuilder = new StringBuilder();
            var promptHeader = "这是我目前使用的论文：";

            promptBuilder.AppendLine(promptHeader);
            foreach (var item in _existArticles)
                promptBuilder.AppendLine($"{item.Message?.Title?[0]} (DOI:{item.Message?.Doi})"); ;
            var promptFooter = "请帮我寻找相似的论文，返回要求：每行一个DOI号，不含有任何其他信息。";
            promptBuilder.AppendLine(promptFooter);

            // build necessary information
            var mainWindow = Application.Current.MainWindow as MainWindow;
            var config = mainWindow!.Config;
            var deepSeekApi = new DeepSeekApi(config.DeepSeekApiKey);
            var conversationHistory = new List<Utils.Api.DeepSeekApi.Message>();

            if (config.DeepSeekApiKey == "empty")
            {
                mainWindow.ShowToast("您还未配置Key，请先配置");
                return;
            }

            // add chat history
            conversationHistory.Add(new Utils.Api.DeepSeekApi.Message
            {
                Role = "system",
                Content = "你是一个精通科研文献检索的助手，你的任务是帮助根据用户已有文献检索相似文献"
            });
            conversationHistory.Add(new Utils.Api.DeepSeekApi.Message
            {
                Role = "user",
                Content = promptBuilder.ToString()
            });

            // request
            SearchProgressBar.Visibility = Visibility.Visible;
            var response = await deepSeekApi.CreateChatCompletionAsync(
                new DeepSeekApi.ChatCompletionRequest
                {
                    Model = "deepseek-chat",
                    Messages = conversationHistory,
                    Temperature = 0.7,
                });

            var content = response?.Choices?[0].Message?.Content;
            if (content is null)
            {
                mainWindow.ShowToast("请求失败，请检查您的网络连接");
                return;
            }
            var dois = content.Split('\n');

            // build articles information
            var httpClient = new HttpClient();
            var findArticles = new List<JournalArticle>();
            ClearArticles();

            foreach (var doi in dois)
            {
                try
                {
                    using var httpResponse = await httpClient.GetAsync($"https://api.crossref.org/works/{doi}");
                    httpResponse.EnsureSuccessStatusCode();
                    var source = await httpResponse.Content.ReadAsStringAsync();

                    var journalArticle = JsonSerializer.Deserialize<JournalArticle>(source);
                    if (journalArticle is null || journalArticle.Message is null)
                    {
                        mainWindow.ShowToast($"[{doi}] 获取失败");
                        continue;
                    }

                    journalArticle.Message.AfterWards();
                    findArticles.Add(journalArticle);
                }
                catch (Exception ex)
                {
                    LogException.Collect(ex, LogException.ExceptionLevel.Warning);
                    mainWindow.ShowToast($"[{doi}] 获取失败");
                }
            }

            SearchProgressBar.Visibility = Visibility.Collapsed;
            AddArticles(findArticles);
        }

        private void ViewArticleDetails(JournalArticle article)
        {
            MessageBox.Show($"查看文献: {article.Message.Title[0]}\n\n摘要: {article.Message.Abstract}");
        }

        private void DownloadArticlePdf(JournalArticle article)
        {
            var pdfLink = article.Message.Link?.FirstOrDefault(link => link.Url.Contains("pdf"))?.Url;
            if (!string.IsNullOrEmpty(pdfLink))
            {
                try
                {
                    // Download pdf
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = pdfLink,
                        UseShellExecute = true
                    });
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"打开链接失败: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("该文献没有可用的PDF链接");
            }
        }

        private void SaveDetailsButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            if (button != null)
            {
                var article = button.DataContext as JournalArticle;
                if (article != null)
                {
                    SaveArticle(article);
                }
            }
        }

        private void SaveArticle(JournalArticle article)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            var detailPage = new DetailPage(article, true);
            mainWindow?.NavigateWithSlideAnimation(detailPage);
        }
    }
}
using Citation.Model.Reference;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Citation.Utils;

namespace Citation.View.Page
{
    public partial class CitationPage : UserControl, INotifyPropertyChanged
    {
        public string? CiteUri { get; set; }

        public string? Status
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CitationPage()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            PlaceholderText.Visibility = string.IsNullOrEmpty(textBox?.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void Cite_Click(object sender, RoutedEventArgs e)
        {
            string source;
            Status = "获取数据中...";

            try
            {
                var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync($"https://api.crossref.org/works/{CiteUri}");
                response.EnsureSuccessStatusCode();
                source = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                LogException.Collect(ex,LogException.ExceptionLevel.Info);
                Status = "获取失败，请检查您的网络环境或文章链接。";
                return;
            }

            var journalArticle = JsonSerializer.Deserialize<JournalArticle>(source);
            if (journalArticle is null || journalArticle.Message is null)
            {
                MainWindow.This.ShowToast("获取失败，请检查您的网络环境或文章链接。");
                return;
            }

            journalArticle.Message.AfterWards();
            Status = $"【{journalArticle!.Message!.Title![0]}】 获取成功";

            var detailPage = new DetailPage(journalArticle);
            MainWindow.This.NavigateWithSlideAnimation(detailPage);
        }
    }
}
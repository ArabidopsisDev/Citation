using Citation.Model;
using Citation.Utils.Api;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Citation.View.Page
{
    /// <summary>
    /// Interaction logic for NLFindPage.xaml
    /// </summary>
    public partial class NLFindPage : UserControl
    {
        public NLFindPage()
        {
            InitializeComponent();
        }

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            string dataBase;

            if (CnkiRadioButton.IsChecked == true)
                dataBase = "知网";
            else
                dataBase = "Web of science";

            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine($"I am using the {dataBase} database for literature review");
            promptBuilder.AppendLine("""
                Next, I will tell you what kind of documents the 
                user wants to find. This is the data class used to read JSON in my code. 
                You need to return a JSON and fill in part of it using the advanced search 
                formula according to the documents the user wants to find. 
                Try to find the documents the user needs. 
                You don't need to fill in as many fields as possible, 
                just as precise as possible.
                """);
            promptBuilder.AppendLine("""
                public class SearchExpression
                {
                    [JsonPropertyName("terms")]
                    public string? Terms { get; set; }

                    [JsonPropertyName("journal-or-booktitle")]
                    public string? Journal { get; set; }

                    [JsonPropertyName("years")]
                    public string? Years { get; set; }

                    [JsonPropertyName("authors")]
                    public string? Authors { get; set; }

                    [JsonPropertyName("affiliations")]
                    public string? Affiliations { get; set; }

                    [JsonPropertyName("volumes")]
                    public string? Volumes { get; set; }

                    [JsonPropertyName("pages")]
                    public string? Pages { get; set; }

                    [JsonPropertyName("issues")]
                    public string? Issues { get; set; }

                    [JsonPropertyName("title-abstract-or-author-specified-keywords")]
                    public string? KeyWords { get; set; }

                    [JsonPropertyName("title")]
                    public string? Title { get; set; }

                    [JsonPropertyName("references")]
                    public string? References { get; set; }

                    [JsonPropertyName("issn-or-isbn")]
                    public string? ISSNorISBN { get; set; }
                }
                """);
            promptBuilder.AppendLine("""
                For the Sciencedirect database, the page number, volume number, 
                year, and page number from x to y are formatted as x-y.
                """);
            promptBuilder.AppendLine("""
                Please ensure that all fields are constructed 
                in English and try to combine multiple academic terms. For example, 
                for walnut anthracnose, try "Juglans regia OR walnut". You can use 
                advanced search terms such as Boolean expressions to ensure 
                accurate search. Each field can use up to eight Boolean connectors, 
                including AND, OR, and NOT.
                """);
            promptBuilder.AppendLine("""
                For example, for the prevention and treatment 
                of walnut anthracnose, you should first split the keywords into "walnut",
                "anthracnose" and "prevention OR treatment", and then connect them 
                according to natural logical relationships after searching for synonyms. 
                If a field exceeds eight Boolean connectors, you should find a way to 
                split it into other fields or reduce the search as appropriate.
                """);
            promptBuilder.AppendLine("The document the user wants to find is:");
            promptBuilder.Append(PromptInput.Text);

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
                Content = "You are an assistant who is proficient in scientific research " +
                "literature retrieval. Your task is to help construct advanced searches " +
                "based on the literature that users want to find."
            });
            conversationHistory.Add(new Utils.Api.DeepSeekApi.Message
            {
                Role = "user",
                Content = promptBuilder.ToString()
            });

            // request
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

            var expression = JsonSerializer.Deserialize<SearchExpression>(content)!.ToScienceDirect();
            var processStartInfo = new ProcessStartInfo
            {
                FileName = expression,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(processStartInfo);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            PlaceholderText.Visibility = string.IsNullOrEmpty(textBox?.Text) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

using Citation.Model;
using Citation.Utils.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Citation.Controls
{
    public partial class AIDesign : UserControl
    {

        private List<ChatMessage> messages = new List<ChatMessage>();
        public event EventHandler CloseRequested;
        public event EventHandler ExperimentGenerated;
        public string ExperimentFile { get; private set; } = "";

        public AIDesign()
        {
            InitializeComponent();

            // Add welcome info
            AddAIMessage("您好！我是田间试验设计AI助手。我可以帮助您设计随机区组、裂区等试验方案。请问您需要什么帮助？");
        }

        private void AddAIMessage(string message)
        {
            messages.Add(new ChatMessage { Content = message, Type = "ai" });
            AddMessageToUI(message, "ai");
        }

        private void AddUserMessage(string message)
        {
            messages.Add(new ChatMessage { Content = message, Type = "user" });
            AddMessageToUI(message, "user");
        }

        private void AddMessageToUI(string message, string type)
        {
            // build message border
            var messageBorder = new Border
            {
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 6, 0, 0)
            };

            if (type == "user")
            {
                messageBorder.Background = new SolidColorBrush(Color.FromRgb(99, 102, 241));
                messageBorder.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                messageBorder.Background = new SolidColorBrush(Color.FromRgb(227, 232, 240));
                messageBorder.HorizontalAlignment = HorizontalAlignment.Left;
            }

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = type == "user" ? Brushes.White : new SolidColorBrush(Color.FromRgb(51, 65, 85))
            };

            // add to box
            messageBorder.Child = textBlock;
            ChatMessagesPanel.Children.Add(messageBorder);

            // go buttom
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            if (ChatScrollViewer != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ChatScrollViewer.ScrollToEnd();
                }), DispatcherPriority.ContextIdle);
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && SendButton.IsEnabled)
            {
                SendMessage();
                e.Handled = true;
            }
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SendButton.IsEnabled = !string.IsNullOrWhiteSpace(InputTextBox.Text);
        }

        private async void SendMessage()
        {
            var message = InputTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(message))
            {
                AddUserMessage(message);
                InputTextBox.Text = "";

                LoadingIndicator.Visibility = Visibility.Visible;

                var response = await GenerateAIResponse(message);
                if (response != null)
                {
                    var responseArray = response.Split('~');
                    AddAIMessage(responseArray[0]);

                    var jsonString = responseArray[1];
                    if (jsonString.Contains("json")) jsonString = jsonString.Split("json")[1].Split("```")[0];
                    ExperimentFile = jsonString;
                    ExperimentGenerated(this, EventArgs.Empty);
                }
                else
                {
                    AddAIMessage("请求失败，请稍后重试。");
                }

                LoadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<string?> GenerateAIResponse(string userMessage)
        {
            // build necessary information
            var mainWindow = Application.Current.MainWindow as MainWindow;
            var config = mainWindow!.Config;
            var deepSeekApi = new DeepSeekApi(config.DeepSeekApiKey);
            var conversationHistory = new List<Utils.Api.DeepSeekApi.Message>();

            if (config.DeepSeekApiKey == "empty")
            {
                mainWindow.ShowToast("您还未配置Key，请先配置");
                return null;
            }

            // add chat history
            conversationHistory.Add(new Utils.Api.DeepSeekApi.Message
            {
                Role = "system",
                Content = "你是一个精通农学大田实验科研的助手，你将为用户设计条理清晰、严格完整的大田实验方案，给出纯文本即可，无需Markdown"
            });

            // God's prompt words
            var genJson = """
                在给出建议后，你需要输出一个~，之后给出一个json，该json将被c#读取，用于生成可视化的实验方案。我将给出关键代码，
                 if (System.Windows.MessageBox.Show("确定要加载实验吗？当前未保存的更改将会丢失。", "确认", System.Windows.MessageBoxButton.YesNo) != System.Windows.MessageBoxResult.Yes)
                 {
                     return;
                 }

                 // 选择加载文件
                 OpenFileDialog openDialog = new OpenFileDialog
                 {
                     Filter = "大田实验文件 (*.fexp)|*.fexp|所有文件 (*.*)|*.*",
                     DefaultExt = ".fexp",
                     Title = "加载实验"
                 };

                 if (openDialog.ShowDialog() == true)
                 {
                     try
                     {
                         // 从JSON反序列化
                         string json = File.ReadAllText(openDialog.FileName);
                         var options = new JsonSerializerOptions
                         {
                             Converters = { new ColorConverter() }
                         };
                	currentExperiment = JsonSerializer.Deserialize<FieldExperiment>(json, options);
                其中，using System.Collections.ObjectModel;

                namespace Citation.Model.Agriculture;

                public class FieldExperiment
                {
                    public string Name { get; set; } = "新实验";
                    public string Description { get; set; } = "";
                    public DateTime CreateDate { get; set; } = DateTime.Now;
                    public string Author { get; set; } = "";
                    public string Institution { get; set; } = "";

                    public enum ExperimentDesignType
                    {
                        CompletelyRandomized,
                        RandomizedCompleteBlock,
                        SplitPlot,
                        LatinSquare
                    }

                    public ExperimentDesignType DesignType { get; set; } = ExperimentDesignType.RandomizedCompleteBlock;

                    public ObservableCollection<Field> Fields { get; set; } = new ObservableCollection<Field>();
                    public ObservableCollection<Treatment> Treatments { get; set; } = new ObservableCollection<Treatment>();
                    public ObservableCollection<Signboard> Signboards { get; set; } = new ObservableCollection<Signboard>();
                }
                using System.Collections.ObjectModel;
                namespace Citation.Model.Agriculture;

                public class Field
                {
                    public string Id { get; set; } = Guid.NewGuid().ToString();
                    public string Name { get; set; } = "新田块";
                    public double X { get; set; }
                    public double Y { get; set; }
                    public double Width { get; set; }
                    public double Height { get; set; }
                    public double Area { get; set; }
                    public string SoilType { get; set; } = "未知";
                    public string Notes { get; set; } = "";

                    public ObservableCollection<Block> Blocks { get; set; } = new ObservableCollection<Block>();
                }using System.Collections.ObjectModel;

                namespace Citation.Model.Agriculture;
                public class Block
                {
                    public string Id { get; set; } = Guid.NewGuid().ToString();
                    public string Name { get; set; } = "新区组";

                    /// <summary>
                    /// Block number
                    /// </summary>
                    public int Number { get; set; }

                    /// <summary>
                    /// Relative to the coordinates of the parent field
                    /// </summary>
                    public double X { get; set; }

                    /// <summary>
                    /// Relative to the coordinates of the parent field
                    /// </summary>
                    public double Y { get; set; }
                    public double Width { get; set; }
                    public double Height { get; set; }

                    /// <summary>
                    /// The ID of the associated field
                    /// </summary>
                    public string FieldId { get; set; } 

                    public ObservableCollection<Plot> Plots { get; set; } = new ObservableCollection<Plot>();
                }using System;
                using System.Collections.Generic;
                using System.Linq;
                using System.Text;
                using System.Threading.Tasks;

                namespace Citation.Model.Agriculture
                {
                    public class Signboard
                    {
                        public string Id { get; set; } = Guid.NewGuid().ToString();
                        public string Content { get; set; } = "新告示牌";
                        public double X { get; set; }
                        public double Y { get; set; }

                        /// <summary>
                        /// The ID of the associated field
                        /// </summary>
                        public string FieldId { get; set; } 
                        public DateTime CreateDate { get; set; } = DateTime.Now;
                    }
                }
                using System.Windows.Media;

                namespace Citation.Model.Agriculture;

                public class Treatment
                {
                    public string Id { get; set; } = Guid.NewGuid().ToString();
                    public string Name { get; set; } = "新处理";
                    public string Description { get; set; } = "";

                    public double Nitrogen { get; set; }
                    public double Phosphorus { get; set; }
                    public double Potassium { get; set; } 
                    public double Irrigation { get; set; }
                    public string Pesticide { get; set; } = "";

                    public Color Color { get; set; } = Colors.LightBlue;
                }，注意不要太靠左上角，例如一个设计：
                                {
                  "Name": "\u65B0\u5B9E\u9A8C",
                  "Description": "",
                  "CreateDate": "2025-09-14T09:48:09.4304186+08:00",
                  "Author": "",
                  "Institution": "",
                  "DesignType": 1,
                  "Fields": [
                    {
                      "Id": "97288f88-dfcf-4259-9609-4b5667ac828b",
                      "Name": "\u7530\u5757 1",
                      "X": 100,
                      "Y": 97.67000000000002,
                      "Width": 201.60000000000002,
                      "Height": 201.59999999999997,
                      "Area": 4.064255999999999,
                      "SoilType": "\u672A\u77E5",
                      "Notes": "",
                      "Blocks": []
                    },
                    {
                      "Id": "a4c777f5-e6bb-43d1-b0b4-d82b66aae430",
                      "Name": "\u7530\u5757 2",
                      "X": 304,
                      "Y": 100.07000000000002,
                      "Width": 197.60000000000002,
                      "Height": 197.60000000000005,
                      "Area": 3.904576000000002,
                      "SoilType": "\u672A\u77E5",
                      "Notes": "",
                      "Blocks": [
                        {
                          "Id": "0e31b9f6-0b84-4467-a566-ece1b097f57b",
                          "Name": "\u533A\u7EC4 1",
                          "Number": 1,
                          "X": 0,
                          "Y": 0,
                          "Width": 96.80000000000001,
                          "Height": 101.60000000000002,
                          "FieldId": "a4c777f5-e6bb-43d1-b0b4-d82b66aae430",
                          "Plots": []
                        },
                        {
                          "Id": "7fe1d514-e29d-4f77-a0e8-2dd42485526a",
                          "Name": "\u533A\u7EC4 2",
                          "Number": 2,
                          "X": 95.20000000000005,
                          "Y": 0,
                          "Width": 100.79999999999995,
                          "Height": 100,
                          "FieldId": "a4c777f5-e6bb-43d1-b0b4-d82b66aae430",
                          "Plots": [
                            {
                              "Id": "28d9502e-61ad-4751-9e04-fc3a33eb385a",
                              "Name": "\u5C0F\u533A 1",
                              "Code": "2-1",
                              "X": 25.600000000000023,
                              "Y": 28,
                              "Width": 46.39999999999998,
                              "Height": 54.39999999999998,
                              "BlockId": "7fe1d514-e29d-4f77-a0e8-2dd42485526a",
                              "TreatmentId": null
                            }
                          ]
                        }
                      ]
                    }
                  ],
                  "Treatments": [
                    {
                      "Id": "e4dcc3e7-ea69-4c0d-8f48-cc83a7ee0db8",
                      "Name": "\u5BF9\u7167\u7EC4",
                      "Description": "\u65E0\u5904\u7406",
                      "Nitrogen": 0,
                      "Phosphorus": 0,
                      "Potassium": 0,
                      "Irrigation": 0,
                      "Pesticide": "",
                      "Color": "#FFD3D3D3"
                    },
                    {
                      "Id": "2a17566a-e6cf-455c-9489-47362d67d1b0",
                      "Name": "\u4F4E\u6C2E\u5904\u7406",
                      "Description": "\u6C2E\u80A5: 50kg/ha",
                      "Nitrogen": 50,
                      "Phosphorus": 0,
                      "Potassium": 0,
                      "Irrigation": 0,
                      "Pesticide": "",
                      "Color": "#FF90EE90"
                    },
                    {
                      "Id": "aba56524-9e34-4849-927f-c5ff5c5da225",
                      "Name": "\u9AD8\u6C2E\u5904\u7406",
                      "Description": "\u6C2E\u80A5: 100kg/ha",
                      "Nitrogen": 100,
                      "Phosphorus": 0,
                      "Potassium": 0,
                      "Irrigation": 0,
                      "Pesticide": "",
                      "Color": "#FF008000"
                    },
                    {
                      "Id": "b465b288-0d0e-4fe7-b435-90320941cc98",
                      "Name": "NPK\u5904\u7406",
                      "Description": "\u6C2E\u78F7\u94BE\u590D\u5408\u80A5",
                      "Nitrogen": 80,
                      "Phosphorus": 40,
                      "Potassium": 60,
                      "Irrigation": 0,
                      "Pesticide": "",
                      "Color": "#FFADD8E6"
                    }
                  ],
                  "Signboards": []
                }注意你的总区域为1600x1600，在绘画田地和区组等的大小时请注意美观
                """;

            conversationHistory.Add(new Utils.Api.DeepSeekApi.Message
            {
                Role = "user",
                Content = userMessage + genJson
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
            return content;
        }

        private void SuggestionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string suggestion)
            {
                InputTextBox.Text = suggestion;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
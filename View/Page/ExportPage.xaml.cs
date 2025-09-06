using Citation.Model;
using Citation.Model.Reference;
using Citation.Utils;
using Citation.View.Controls;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Citation.View.Page
{
    public partial class ExportPage : UserControl
    {
        private object _draggedItem;
        private int _draggedIndex;

        public ExportPage()
        {
            InitializeComponent();

            ArticlesContainer.AlternationCount = int.MaxValue;

            ArticlesContainer.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            ArticlesContainer.Drop += OnDrop;
            ArticlesContainer.AllowDrop = true;
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = FindVisualParent<ArticleItem>((DependencyObject)e.OriginalSource);
            _draggedItem = item.DataContext;
            _draggedIndex = ArticlesContainer.Items.IndexOf(_draggedItem);
            DragDrop.DoDragDrop(item, _draggedItem, DragDropEffects.Move);
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (_draggedItem == null) return;

            var targetItem = FindVisualParent<ArticleItem>((DependencyObject)e.OriginalSource);
            if (targetItem == null || targetItem.DataContext == _draggedItem) return;

            if (ArticlesContainer.ItemsSource is not IList items) return;

            int targetIndex = ArticlesContainer.Items.IndexOf(targetItem.DataContext);
            items.RemoveAt(_draggedIndex);
            items.Insert(targetIndex, _draggedItem);

            ArticlesContainer.Items.Refresh();
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && child is not T)
                child = VisualTreeHelper.GetParent(child);
            return (T)child!;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var exportBuilder = new StringBuilder();
            var isMarkdown = MarkdownButton.IsChecked == true;

            // Load specific formatter
            IFormatter? _formatter = null;
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

            exportBuilder.AppendLine(isMarkdown ? "## Reference" : @"\section{Reference}");

            // Build string
            if (_formatter is null)
            {
                LogException.Collect(new ArgumentException("指定格式化器不存在"),
                    LogException.ExceptionLevel.Warning);

                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow!.ShowToast("格式化失败，原因是您设置了错误的格式化器");
                return;
            }

            for (int i = 0; i < ArticlesContainer.Items.Count; i++)
            {
                var type = _formatter.GetType();
                _formatter = (IFormatter)Activator.CreateInstance(type, 
                    [(JournalArticle)ArticlesContainer.Items[i]!])!;

                exportBuilder.AppendLine(isMarkdown
                    ? $"[{i + 1}] {_formatter.ToMarkdown()}"
                    : $"[{i + 1}] {_formatter.ToLatex()}");
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "reference"
            };

            if (isMarkdown)
            {
                saveFileDialog.Filter = "markdown文本 (*.md)|*.md|所有文件 (*.*)|*.*";
                saveFileDialog.DefaultExt = "md";
            }
            else
            {
                saveFileDialog.Filter = "LaTex格式文本 (*.tex)|*.tex|所有文件 (*.*)|*.*";
                saveFileDialog.DefaultExt = "tex";
            }

            var result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                var filePath = saveFileDialog.FileName;
                var mainWindow = Application.Current.MainWindow as MainWindow;

                try
                {
                    File.WriteAllText(filePath, exportBuilder.ToString());
                    mainWindow?.ShowToast("引用保存成功");
                }
                catch (IOException ex)
                {
                    LogException.Collect(ex, LogException.ExceptionLevel.Warning);
                    mainWindow?.ShowToast("保存失败，可能文件正在使用...");
                }
            }
        }
    }
}

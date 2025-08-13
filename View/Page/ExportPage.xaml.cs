using Citation.View.Controls;
using System.Collections;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Citation.Model.Format;
using Citation.Model.Reference;
using Citation.Utils;

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
            var item = FindVisualParent<ArticleItem>(e.OriginalSource as DependencyObject);
            _draggedItem = item.DataContext;
            _draggedIndex = ArticlesContainer.Items.IndexOf(_draggedItem);
            DragDrop.DoDragDrop(item, _draggedItem, DragDropEffects.Move);
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (_draggedItem == null) return;

            var targetItem = FindVisualParent<ArticleItem>(e.OriginalSource as DependencyObject);
            if (targetItem == null || targetItem.DataContext == _draggedItem) return;

            var items = ArticlesContainer.ItemsSource as IList;
            if (items == null) return;

            int targetIndex = ArticlesContainer.Items.IndexOf(targetItem.DataContext);
            items.RemoveAt(_draggedIndex);
            items.Insert(targetIndex, _draggedItem);

            ArticlesContainer.Items.Refresh();
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T))
                child = VisualTreeHelper.GetParent(child);
            return child as T;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var exportBuilder = new StringBuilder();
            var isMarkdown = MarkdownButton.IsChecked == true;

            exportBuilder.AppendLine(isMarkdown ? "## Reference" : @"\section{Reference}");
            for (int i = 0; i < ArticlesContainer.Items.Count; i++)
            {
                var builder = new Apa((JournalArticle)ArticlesContainer.Items[i]!);
                exportBuilder.AppendLine(isMarkdown
                    ? $"[{i + 1}] {builder.ToMarkdown()}"
                    : $"[{i + 1}] {builder.ToLatex()}");
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.FileName = "reference";
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
                try
                {
                    File.WriteAllText(filePath, exportBuilder.ToString());
                    MainWindow.This.ShowToast("引用保存成功");
                }
                catch (IOException ex)
                {
                    LogException.Collect(ex, LogException.ExceptionLevel.Warning);
                    MainWindow.This.ShowToast("保存失败，可能文件正在使用...");
                }
            }
        }
    }
}

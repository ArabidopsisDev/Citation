using Citation.View.Controls;
using System.Collections;
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
            var item = FindVisualParent<ArticleItem>(e.OriginalSource as DependencyObject);
            if (item != null)
            {
                _draggedItem = item.DataContext;
                _draggedIndex = ArticlesContainer.Items.IndexOf(_draggedItem);
                DragDrop.DoDragDrop(item, _draggedItem, DragDropEffects.Move);
            }
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
            bool isMarkdown = MarkdownButton.IsChecked == true;
        }
    }
}
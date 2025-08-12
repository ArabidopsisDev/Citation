using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Citation.View.Controls
{
    public partial class ArticleItem : UserControl
    {
        public ArticleItem()
        {
            InitializeComponent();
            ExpandButton.MouseLeftButtonDown += ExpandButton_MouseLeftButtonDown;
        }

        // 添加序号属性
        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register("Index", typeof(int), typeof(ArticleItem),
                new PropertyMetadata(0, OnIndexChanged));

        public int Index
        {
            get => (int)GetValue(IndexProperty);
            set => SetValue(IndexProperty, value);
        }

        private static void OnIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArticleItem item && e.NewValue is int index)
            {
                item.IndexText.Text = (index + 1).ToString();
            }
        }

        private void ExpandButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ToggleDetails();
            e.Handled = true;
        }

        private void ToggleDetails()
        {
            if (DetailPanel.Visibility == Visibility.Visible)
            {
                DetailPanel.Visibility = Visibility.Collapsed;
                ExpandButton.Text = "▼";
            }
            else
            {
                DetailPanel.Visibility = Visibility.Visible;
                ExpandButton.Text = "▲";
            }
        }
    }
}
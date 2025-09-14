using Citation.Model.Reference;
using System.Windows;
using System.Windows.Controls;

namespace Citation.View.Page
{
    /// <summary>
    /// Interaction logic for DetailPage.xaml
    /// </summary>
    public partial class DetailPage : UserControl
    {
        public JournalArticle Article { get; set; }
        private bool _canBack = false;

        public DetailPage(JournalArticle article, bool canBack = false)
        {
            InitializeComponent();

            Article = article;
            _canBack = canBack;
            DataContext = this;

            if (!_canBack)
                GoBack.Visibility = Visibility.Collapsed;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var insertCommand = Article.ToSql();
            Acceed.Shared.Execute(insertCommand);
            var mainWindow = Application.Current.MainWindow as MainWindow;

            if (!_canBack)
                mainWindow?.NavigateWithSlideAnimation(new Citation.View.Page.ViewReferencePage());
            else
                mainWindow?.ShowToast("文献已保存");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.MainFrame.RemoveBackEntry();
        }

        private void GoBack_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_canBack)
                return;

            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.MainFrame.GoBack();
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using Citation.Model.Reference;

namespace Citation.View.Page
{
    /// <summary>
    /// Interaction logic for DetailPage.xaml
    /// </summary>
    public partial class DetailPage : UserControl
    {
        public JournalArticle Article { get; set; }

        public DetailPage(JournalArticle article)
        {
            InitializeComponent();

            Article = article;
            DataContext = this;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var insertCommand = Article.ToSql();
            Acceed.Shared.FeBr(insertCommand);

            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.NavigateWithSlideAnimation(new Citation.View.Page.ViewReferencePage());
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.MainFrame.RemoveBackEntry();
        }
    }
}

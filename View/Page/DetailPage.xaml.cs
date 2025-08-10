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
            Acceed.Shared.Execute(insertCommand);
        }
    }
}

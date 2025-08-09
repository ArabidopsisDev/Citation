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
    }
}

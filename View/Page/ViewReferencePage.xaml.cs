using Citation.Model.Reference;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Citation.View.Page
{
    public partial class ViewReferencePage : UserControl
    {
        public ViewReferencePage()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is JournalArticle article)
            {
                ((dynamic)DataContext).SelectedArticle = article;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void ViewReferencePage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var reader = Acceed.Shared.Query("SELECT * FROM tb_Paper");

        }
    }
}
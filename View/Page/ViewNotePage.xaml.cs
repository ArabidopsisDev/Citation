using Citation.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Citation.View.Page
{
    /// <summary>
    /// Interaction logic for ViewNotePage.xaml
    /// </summary>
    public partial class ViewNotePage : UserControl
    {
        public ObservableCollection<Note> Notes { get; set; } = new ObservableCollection<Note>();

        public ViewNotePage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Notes.Clear();
            var reader = Acceed.Shared.Query("SELECT * FROM tb_Note");
            while (reader.Read())
                Notes.Add(Note.FromSql(reader)!);
        }

        private void EditButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Note note)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.NavigateWithSlideAnimation(new AddNotePage(note));
            }
        }

        private void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Note note)
            {
                note.DeleteSql(Acceed.Shared.Connection);
                Notes.Remove(note);

                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow!.ShowToast("删除便签成功");
            }
        }
    }
}
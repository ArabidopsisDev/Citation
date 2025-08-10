using System.Collections.ObjectModel;

namespace Citation.Model.Reference
{
    public class FolderItem
    {
        public string Name { get; set; }
        public ObservableCollection<JournalArticle> Articles { get; set; } = new ObservableCollection<JournalArticle>();
    }
}
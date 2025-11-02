using System.Collections.ObjectModel;

namespace Citation.Model.Reference;

/// <summary>
/// Represents a folder that contains a collection of journal articles.
/// </summary>
public class FolderItem
{
    public string Name { get; set; }
    public ObservableCollection<JournalArticle> Articles { get; set; } = new ObservableCollection<JournalArticle>();
}
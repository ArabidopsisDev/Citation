using Citation.Properties;
using System.Collections.ObjectModel;

namespace Citation.Model.Agriculture;
public class Block
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = Resources.BlockData_DefaultName;

    /// <summary>
    /// Block number
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Relative to the coordinates of the parent field
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Relative to the coordinates of the parent field
    /// </summary>
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    /// <summary>
    /// The ID of the associated field
    /// </summary>
    public string FieldId { get; set; } 

    public ObservableCollection<Plot> Plots { get; set; } = new ObservableCollection<Plot>();
}
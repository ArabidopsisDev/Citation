using Citation.Properties;
using System.Collections.ObjectModel;
namespace Citation.Model.Agriculture;

public class Field
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = Resources.FieldData_DefaultName;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Area { get; set; }
    public string SoilType { get; set; } = Resources.PublicData_Unknown;
    public string Notes { get; set; } = "";

    public ObservableCollection<Block> Blocks { get; set; } = new ObservableCollection<Block>();
}
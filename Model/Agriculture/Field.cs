using System.Collections.ObjectModel;
namespace Citation.Model.Agriculture;

public class Field
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "新田块";
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Area { get; set; }
    public string SoilType { get; set; } = "未知";
    public string Notes { get; set; } = "";

    public ObservableCollection<Block> Blocks { get; set; } = new ObservableCollection<Block>();
}
using Citation.Properties;
using System.Windows.Media;

namespace Citation.Model.Agriculture;

public class Treatment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = Resources.TreatmentData_DefaultName;
    public string Description { get; set; } = "";

    public double Nitrogen { get; set; }
    public double Phosphorus { get; set; }
    public double Potassium { get; set; } 
    public double Irrigation { get; set; }
    public string Pesticide { get; set; } = "";

    public Color Color { get; set; } = Colors.LightBlue;
}

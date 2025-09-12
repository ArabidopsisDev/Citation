namespace Citation.Model.Agriculture;

public class Plot
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "新小区";

    /// <summary>
    /// Community number, such as "A-01"
    /// </summary>
    public string Code { get; set; } = "";

    /// <summary>
    /// Coordinates relative to the parent group
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Coordinates relative to the parent group
    /// </summary>
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    /// <summary>
    /// The ID of the associated group
    /// </summary>
    public string? BlockId { get; set; }

    /// <summary>
    /// Assigned processing ID
    /// </summary>
    public string? TreatmentId { get; set; } 
}

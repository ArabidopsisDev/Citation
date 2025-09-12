namespace Citation.Model;

public class TimelineGroup
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<TimelineItem> Items { get; set; } = new();
}

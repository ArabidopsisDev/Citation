using System.Windows.Media;

namespace Citation.Utils;

public static class Extends
{
    public static Color AdjustBrightness(this Color color, double factor)
    {
        return Color.FromRgb(
            (byte)(color.R * factor),
            (byte)(color.G * factor),
            (byte)(color.B * factor)
        );
    }
}

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Citation.View.Converter;

public class MessageTypeToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string type)
        {
            return type == "user" ? new SolidColorBrush(Colors.White) :
                new SolidColorBrush(Color.FromRgb(51, 65, 85));
        }
        return new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

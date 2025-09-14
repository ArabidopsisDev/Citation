using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Citation.View.Converter;

public class MessageTypeToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string type)
        {
            return type == "user" ? new SolidColorBrush(Color.FromRgb(99, 102, 241)) :
                new SolidColorBrush(Color.FromRgb(227, 232, 240));
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
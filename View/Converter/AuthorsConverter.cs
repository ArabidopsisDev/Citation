using System.Globalization;
using System.Windows.Data;

namespace Citation.View.Converter;

public class AuthorsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string[] authors && authors.Length > 0)
        {
            return string.Join(", ", authors.Take(3)) + (authors.Length > 3 ? "等" : "");
        }
        return "未知作者";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

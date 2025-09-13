using Citation.Model.Reference;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Citation.View.Converter;

public class HasPdfLinkConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Link[] links)
        {
            return links.Any(link => link.Url.Contains("pdf")) ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

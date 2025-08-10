using Citation.Model.Reference;
using System.Globalization;
using System.Windows.Data;

namespace Citation.Model.Convert
{
    internal class DateConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null) return null;

            var published = (Published)value;
            var dateTime = new DateTime(published.DateParts[0][0], published.DateParts[0][1],
                published.DateParts[0][2]);
            return dateTime;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null) return null;

            var dateTime = (DateTime)value;
            int[][] published = [[dateTime.Year, dateTime.Month, dateTime.Day]];
            return published;
        }
    }
}

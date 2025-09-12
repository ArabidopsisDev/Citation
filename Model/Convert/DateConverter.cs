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

            if (published.DateParts is null) return new DateTime(1145, 1, 14);

            DateTime dateTime;
            if (published.DateParts[0].Length > 2)
                dateTime = new DateTime(published.DateParts[0][0], published.DateParts[0][1],
                   published.DateParts[0][2]);
            else
                dateTime = new DateTime(published.DateParts[0][0], published.DateParts[0][1],
                    1);
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

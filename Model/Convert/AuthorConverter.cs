using System.Globalization;
using System.Text;
using System.Windows.Data;
using Citation.Model.Reference;

namespace Citation.Model.Convert
{
    class AuthorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var authorsBuilder = new StringBuilder();
            if (value is null) return "Anonymous";

            foreach (var author in (Author[])value)
                authorsBuilder.AppendLine(author.ToString());
            var result = authorsBuilder.ToString().TrimEnd();
            return result;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Is this method really useful? 0 people care.
            return value?.ToString()?
               .Split('\n')
               .Select(Author.ConvertBack)
               .ToArray();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Citation.View.Converter;

public class DateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int[] dateParts = (int[])value;
        if (dateParts.Length >= 3)
            return $"{dateParts[0]}.{dateParts[1]:D2}.{dateParts[2]:D2}";
        else if (dateParts.Length >= 2)
            return $"{dateParts[0]}.{dateParts[1]:D2}";
        else if (dateParts.Length >= 2)
            return $"{dateParts[0]}";
        return "未知日期";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

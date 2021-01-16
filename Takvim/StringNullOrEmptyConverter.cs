using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Takvim
{
    public class StringNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => string.IsNullOrWhiteSpace((string)value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
using System;
using System.Globalization;
using System.Windows.Data;

namespace Takvim
{
    public class NullableToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => parameter != null ? value == null : value != null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
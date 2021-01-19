using System;
using System.Globalization;
using System.Windows.Data;

namespace Takvim
{
    public class IntBooleanZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is int Id && Id != 0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
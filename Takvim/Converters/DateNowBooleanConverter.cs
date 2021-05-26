using System;
using System.Globalization;
using System.Windows.Data;

namespace Takvim
{
    public class DateNowBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is DateTime date && date == DateTime.Today;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
using System;
using System.Globalization;
using System.Windows.Data;

namespace Takvim
{
    public class DateAnimationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => values[0] is DateTime dateTime && values[1] is DateTime animationdate && dateTime == animationdate;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
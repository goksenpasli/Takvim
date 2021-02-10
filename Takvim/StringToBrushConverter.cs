using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Takvim
{
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is string renk ? (SolidColorBrush)new BrushConverter().ConvertFromString(renk) : null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => (value as SolidColorBrush)?.ToString();
    }
}
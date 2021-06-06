using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Takvim
{
    public class RamadanDateToVisibilityConverter : IValueConverter
    {
        public RamadanDateToVisibilityConverter()
        {
            HijriCalendar = new HijriCalendar();
            HijriCalendar.HijriAdjustment--;
        }

        public HijriCalendar HijriCalendar { get; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is DateTime date && date > HijriCalendar.MinSupportedDateTime) ? (HijriCalendar.GetMonth(date) == 9) ? Visibility.Visible : Visibility.Collapsed : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
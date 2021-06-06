using System;
using System.Globalization;
using System.Windows.Data;

namespace Takvim
{
    public class RamadanDateToHolidayConverter : RamadanDateToVisibilityConverter, IValueConverter
    {
        public new object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is DateTime date && date > HijriCalendar.MinSupportedDateTime) ? (HijriCalendar.GetMonth(date) == 10 && (HijriCalendar.GetDayOfMonth(date) == 1 || HijriCalendar.GetDayOfMonth(date) == 2 || HijriCalendar.GetDayOfMonth(date) == 3)) ? true : (HijriCalendar.GetMonth(date) == 9 && (HijriCalendar.GetDayOfMonth(date) == 30)) ? null : false : false;
        }

        public new object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
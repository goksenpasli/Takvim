using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Takvim
{
    public class MonthToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int month)
            {
                if (DateTime.Now.Month == month)
                {
                    return Brushes.DarkOrange;
                }
                if (DateTime.Now.Month > month)
                {
                    return Brushes.Gray;
                }
                if (DateTime.Now.Month < month)
                {
                    return Brushes.Green;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
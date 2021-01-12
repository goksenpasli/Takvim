using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Takvim
{

    public class ŞuAnkiAyColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is string ay ? ay == DateTime.Now.ToString("MMMM") ? Brushes.Green : Brushes.Red : Brushes.Green;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
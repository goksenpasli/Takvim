using System;
using System.Globalization;
using System.Windows.Data;

namespace Takvim
{
    public class Base64ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value is string base64image) ? System.Convert.FromBase64String(base64image).WebpDecode(double.TryParse((string)parameter, out double res) ? res : 0) : null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
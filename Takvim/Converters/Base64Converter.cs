using System;
using System.Globalization;
using System.Windows.Data;

namespace Takvim
{
    public class Base64Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value is string base64data) ? System.Convert.FromBase64String(base64data) : null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
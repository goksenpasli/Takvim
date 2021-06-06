using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Extensions
{
    public class WebpByteArrayToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DesignerProperties.GetIsInDesignMode(new DependencyObject()) ? null : value is byte[] webpbyte ? webpbyte.WebpDecode(double.TryParse((string)parameter, out double res) ? res : 0) : DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
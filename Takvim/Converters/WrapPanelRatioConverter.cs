using System;
using System.Globalization;
using System.Windows.Data;

namespace Takvim
{
    public class WrapPanelRatioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is short ratio) ? System.Windows.SystemParameters.WorkArea.Width / ratio : System.Windows.SystemParameters.WorkArea.Width / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Takvim
{

    public class Shell32FileIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string index)
            {
                try
                {
                    return ExtensionMethods.IconCreate($"{Environment.SystemDirectory}\\Shell32.dll", System.Convert.ToInt32(index));
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
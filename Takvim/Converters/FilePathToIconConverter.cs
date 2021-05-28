using System;
using System.Globalization;
using System.Windows.Data;
using static Takvim.ExtensionMethods;

namespace Takvim
{
    public class FilePathToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return value is string path ? path.IconCreate(IconSize.Large) : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
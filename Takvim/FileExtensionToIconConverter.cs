using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{
    public class FileExtensionToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return Path.GetExtension(value as string).IkonOluştur();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
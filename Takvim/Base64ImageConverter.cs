using System;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Takvim
{
    public class Base64ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string base64image)
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.None;
                bi.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bi.DecodePixelHeight = int.TryParse((string)parameter, out int res) ? res : 96;
                bi.StreamSource = new MemoryStream(System.Convert.FromBase64String(base64image));
                bi.EndInit();
                bi.Freeze();
                return bi;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Globalization;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{
    public class DateEndTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is XmlNode xmlnode)
            {
                DateTime.TryParseExact(xmlnode.Attributes?.GetNamedItem("SaatBaslangic")?.Value, "H:m", new CultureInfo("tr-TR"), DateTimeStyles.None, out DateTime saatbaslangic);
                double.TryParse(xmlnode.Attributes?.GetNamedItem("Saat")?.Value, out double saat);
                return $"{saatbaslangic:HH:mm}-{saatbaslangic.AddHours(saat):HH:mm}";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
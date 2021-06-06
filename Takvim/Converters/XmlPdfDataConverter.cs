using System;
using System.Globalization;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{

    public class XmlPdfDataConverter : IValueConverter
    {
        private readonly Base64Converter base64Converter;

        public XmlPdfDataConverter()
        {
            base64Converter = new Base64Converter();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is XmlNode xmldata ? base64Converter.Convert(xmldata["Pdf"]?.InnerText, null, null, CultureInfo.CurrentCulture) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
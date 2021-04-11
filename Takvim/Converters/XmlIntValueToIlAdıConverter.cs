using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{
    public class XmlIntValueToIlAdıConverter : IValueConverter
    {
        private readonly XmlDataProvider xmlDataProvider;

        public XmlIntValueToIlAdıConverter()
        {
            xmlDataProvider = new XmlDataProvider
            {
                IsInitialLoadEnabled = true,
                IsAsynchronous = false
            };
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int xmlintvalue)
            {
                xmlDataProvider.Source = new Uri($"https://www.namazvakti.com/XML.php?cityID={xmlintvalue}");
                return xmlDataProvider.Document?.SelectNodes("/cityinfo").Cast<XmlNode>().FirstOrDefault()?.Attributes.GetNamedItem("cityNameTR").Value;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
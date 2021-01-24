using System;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{
    public class XmlDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => XmlConvert.ToDateTime(value as string, XmlDateTimeSerializationMode.Local);

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.Local);
    }
}
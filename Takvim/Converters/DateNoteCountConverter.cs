using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{
    public class DateNoteCountConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is DateTime dateTime && MainViewModel.xmlDataProvider?.Data is ICollection<XmlNode> xmlNode)
            {
                int adet = xmlNode.Count(z => DateTime.Parse(z["Gun"]?.InnerText) == dateTime);
                if (adet > 0)
                {
                    return adet;
                }
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
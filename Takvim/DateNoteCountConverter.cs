using System;
using System.Globalization;
using System.Windows.Data;
using System.Linq;
using System.Xml;
using System.Collections.Generic;

namespace Takvim
{
    public class DateNoteCountConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is DateTime dateTime && values[1] is XmlDataProvider xmldataprovider )
            {
                var adet = (xmldataprovider.Data as ICollection<XmlNode>)?.Count(z => !string.IsNullOrWhiteSpace(z["Aciklama"].InnerText) && DateTime.Parse(z["Gun"].InnerText) == dateTime);
                return adet == 0 ? null : adet;
            }
            else
            {
                return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
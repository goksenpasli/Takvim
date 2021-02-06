using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{
    public class DateNoteCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Data data)
            {
                if (MainViewModel.xmlDataProvider?.Data is ICollection<XmlNode> xmlNode)
                {
                    data.VeriSayısı = xmlNode.Count(z => DateTime.Parse(z["Gun"]?.InnerText) == data.TamTarih);
                }
                return data;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{
    public class SelectedDateToDataListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Data data && MainViewModel.xmlDataProvider?.Data is ICollection<XmlNode> xmlNode)
            {
                return xmlNode.Where(z => DateTime.Parse(z["Gun"]?.InnerText) == data.TamTarih);
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
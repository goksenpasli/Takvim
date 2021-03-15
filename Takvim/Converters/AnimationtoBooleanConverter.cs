using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{
    public class AnimationtoBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is DateTime dateTime && MainViewModel.xmlDataProvider?.Data is ICollection<XmlNode> xmlNode && xmlNode.Count(z => DateTime.Parse(z["Gun"]?.InnerText) == dateTime) > 0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
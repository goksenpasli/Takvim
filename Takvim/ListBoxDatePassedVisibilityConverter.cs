using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{
    public class ListBoxDatePassedVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value is XmlNode xmlNode) ? (DateTime.Parse(xmlNode["Gun"].InnerText) < DateTime.Today) ? Visibility.Collapsed : Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
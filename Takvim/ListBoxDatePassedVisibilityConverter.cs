using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{
    public class ListBoxDatePassedVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is XmlNode xmlNode)
            {
                if (DateTime.Parse(xmlNode["Gun"].InnerText) < DateTime.Today)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
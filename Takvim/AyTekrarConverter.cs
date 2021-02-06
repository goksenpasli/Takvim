using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{
    public class AyTekrarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Data data && MainViewModel.xmlDataProvider?.Data is ICollection<XmlNode> xmlNode)
            {
                foreach (XmlNode item in xmlNode.Where(z => z.Attributes["AyTekrar"]?.InnerText == "true"))
                {
                    if (data.TamTarih.Day == DateTime.Parse(item["Gun"].InnerText).Day && data.TamTarih > DateTime.Today)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
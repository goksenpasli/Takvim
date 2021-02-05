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
        private readonly ICollection<XmlNode> xmlNode;

        public AyTekrarConverter() => xmlNode = MainViewModel.xmlDataProvider?.Data as ICollection<XmlNode>;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Data data)
            {
                IEnumerable<XmlNode> liste = xmlNode?.Where(z => z.Attributes["AyTekrar"].InnerText == "true");
                if (liste != null)
                {
                    foreach (XmlNode item in liste)
                    {
                        return data.TamTarih.Day == DateTime.Parse(item["Gun"].InnerText).Day && data.TamTarih > DateTime.Today;
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
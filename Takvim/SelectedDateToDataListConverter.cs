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
        private readonly ICollection<XmlNode> xmlNode;

        public SelectedDateToDataListConverter() => xmlNode = MainViewModel.xmlDataProvider?.Data as ICollection<XmlNode>;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is Data data ? (xmlNode?.Where(z => DateTime.Parse(z["Gun"].InnerText) == data.TamTarih).Select(z => z["Aciklama"])) : null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
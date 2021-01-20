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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is Data data ? ((MainViewModel.xmlDataProvider?.Data as ICollection<XmlNode>)?.Where(z => DateTime.Parse(z["Gun"].InnerText) == data.TamTarih)) : null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }   
}
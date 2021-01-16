﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{
    public class DateNoteCountConverter : IValueConverter
    {
        private readonly ICollection<XmlNode> xmlNode;

        public DateNoteCountConverter() => xmlNode = MainViewModel.xmlDataProvider?.Data as ICollection<XmlNode>;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Data data)
            {
                var adet = xmlNode?.Count(z => !string.IsNullOrWhiteSpace(z["Aciklama"].InnerText) && DateTime.Parse(z["Gun"].InnerText) == data.TamTarih);
                data.VeriSayısı = (int)adet;
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
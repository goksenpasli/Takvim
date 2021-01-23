using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Takvim
{
    public class YarımGünTatilConverter : IValueConverter
    {
        private readonly List<Tuple<short, short, string>> Tatiller = new List<Tuple<short, short, string>>();

        public YarımGünTatilConverter() => Tatiller.Add(new Tuple<short, short, string>(10, 28, "Cumhuriyet Bayramı"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is Data data && Tatiller.Any(z => z.Item1 == data.TamTarih.Month && z.Item2 == data.TamTarih.Day);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
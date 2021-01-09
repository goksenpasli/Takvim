using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Takvim
{
    public class GünTatilConverter : IValueConverter
    {
        private readonly List<Tuple<string, int>> tatiller = new List<Tuple<string, int>>();

        public GünTatilConverter()
        {
            tatiller.Add(new Tuple<string, int>("Ocak", 1));
            tatiller.Add(new Tuple<string, int>("Nisan", 23));
            tatiller.Add(new Tuple<string, int>("Mayıs", 1));
            tatiller.Add(new Tuple<string, int>("Mayıs", 19));
            tatiller.Add(new Tuple<string, int>("Temmuz", 15));
            tatiller.Add(new Tuple<string, int>("Ağustos", 30));
            tatiller.Add(new Tuple<string, int>("Ekim", 29));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is Data data && tatiller.Any(z => z.Item1 == data.Ay && z.Item2 == data.Gün);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
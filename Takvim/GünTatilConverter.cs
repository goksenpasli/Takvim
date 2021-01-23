using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Takvim
{
    public class GünTatilConverter : IValueConverter
    {
        public List<Tuple<short, short, string>> Tatiller { get; } = new List<Tuple<short, short, string>>();

        public GünTatilConverter()
        {
            Tatiller.Add(new Tuple<short, short, string>(1, 1, "Yılbaşı"));
            Tatiller.Add(new Tuple<short, short, string>(4, 23, "Ulusal Egemenlik ve Çocuk Bayramı"));
            Tatiller.Add(new Tuple<short, short, string>(5, 1, "Emek ve Dayanışma Günü"));
            Tatiller.Add(new Tuple<short, short, string>(5, 19, "Atatürk'ü Anma, Gençlik ve Spor Bayramı"));
            Tatiller.Add(new Tuple<short, short, string>(7, 15, "Demokrasi ve Millî Birlik Günü"));
            Tatiller.Add(new Tuple<short, short, string>(8, 30, "Zafer Bayramı"));
            Tatiller.Add(new Tuple<short, short, string>(10, 29, "Cumhuriyet Bayramı"));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is Data data && Tatiller.Any(z => z.Item1 == data.TamTarih.Month && z.Item2 == data.TamTarih.Day);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
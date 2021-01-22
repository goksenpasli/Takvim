using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Takvim
{
    public class GünTatilConverter : IValueConverter
    {
        public List<Tuple<string, short, string>> Tatiller { get; } = new List<Tuple<string, short, string>>();

        public GünTatilConverter()
        {
            Tatiller.Add(new Tuple<string, short, string>("Ocak", 1, "Yılbaşı"));
            Tatiller.Add(new Tuple<string, short, string>("Nisan", 23, "Ulusal Egemenlik ve Çocuk Bayramı"));
            Tatiller.Add(new Tuple<string, short, string>("Mayıs", 1, "Emek ve Dayanışma Günü"));
            Tatiller.Add(new Tuple<string, short, string>("Mayıs", 19, "Atatürk'ü Anma, Gençlik ve Spor Bayramı"));
            Tatiller.Add(new Tuple<string, short, string>("Temmuz", 15, "Demokrasi ve Millî Birlik Günü"));
            Tatiller.Add(new Tuple<string, short, string>("Ağustos", 30, "Zafer Bayramı"));
            Tatiller.Add(new Tuple<string, short, string>("Ekim", 29, "Cumhuriyet Bayramı"));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is Data data && Tatiller.Any(z => z.Item1 == data.Ay && z.Item2 == data.Gün);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
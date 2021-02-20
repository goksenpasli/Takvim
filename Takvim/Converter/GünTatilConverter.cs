using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Takvim
{
    public class GünTatilConverter : IValueConverter
    {
        public List<Tuple<short, short, string, bool>> Tatiller { get; }

        public GünTatilConverter()
        {
            Tatiller = new List<Tuple<short, short, string, bool>>
            {
                new Tuple<short, short, string, bool>(1, 1, "Yılbaşı", true),
                new Tuple<short, short, string, bool>(4, 23, "Ulusal Egemenlik ve Çocuk Bayramı", true),
                new Tuple<short, short, string, bool>(5, 1, "Emek ve Dayanışma Günü", true),
                new Tuple<short, short, string, bool>(5, 19, "Atatürk'ü Anma, Gençlik ve Spor Bayramı", true),
                new Tuple<short, short, string, bool>(7, 15, "Demokrasi ve Millî Birlik Günü", true),
                new Tuple<short, short, string, bool>(8, 30, "Zafer Bayramı", true),
                new Tuple<short, short, string, bool>(10, 28, "Cumhuriyet Bayramı", false),
                new Tuple<short, short, string, bool>(10, 29, "Cumhuriyet Bayramı", true)
            };
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Data data)
            {
                if (Tatiller.Any(z => z.Item1 == data.TamTarih.Month && z.Item2 == data.TamTarih.Day && z.Item4))
                {
                    return true;
                }
                if (Tatiller.Any(z => z.Item1 == data.TamTarih.Month && z.Item2 == data.TamTarih.Day && !z.Item4))
                {
                    return null;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Takvim
{
    public class BilinenGünlerConverter : IValueConverter
    {
        public BilinenGünlerConverter()
        {
            Günler = new List<Tuple<short, short, string>>
            {
                new Tuple<short, short, string>(2, 14, "Sevgililer Günü"),
                new Tuple<short, short, string>(3, 8, "Dünya Kadınlar Günü"),
                new Tuple<short, short, string>(3, 21, "Nevruz Bayramı"),
                new Tuple<short, short, string>(5, 10, "Anneler Günü"),
                new Tuple<short, short, string>(6, 21, "Babalar Günü"),
                new Tuple<short, short, string>(11, 10, "Atatürk’ün Ölüm Günü")
            };
        }

        public List<Tuple<short, short, string>> Günler { get; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Data data && Günler.Any(z => z.Item1 == data.TamTarih.Month && z.Item2 == data.TamTarih.Day);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BilinenGünlerSeçiliConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Data data ? new BilinenGünlerConverter().Günler.Find(z => z.Item1 == data.TamTarih.Month && z.Item2 == data.TamTarih.Day) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Takvim
{
    public class BilinenGünlerConverter : IValueConverter
    {
        public List<Tuple<short, short, string>> Günler { get; } = new List<Tuple<short, short, string>>();

        public BilinenGünlerConverter()
        {
            Günler.Add(new Tuple<short, short, string>(2, 14, "Sevgililer Günü"));
            Günler.Add(new Tuple<short, short, string>(3, 8, "Dünya Kadınlar Günü"));
            Günler.Add(new Tuple<short, short, string>(3, 21, "Nevruz Bayramı"));
            Günler.Add(new Tuple<short, short, string>(5, 10, "Anneler Günü"));
            Günler.Add(new Tuple<short, short, string>(6, 21, "Babalar Günü"));
            Günler.Add(new Tuple<short, short, string>(11, 10, "Atatürk’ün Ölüm Günü"));      
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is Data data && Günler.Any(z => z.Item1 == data.TamTarih.Month && z.Item2 == data.TamTarih.Day);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
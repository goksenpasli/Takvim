﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Takvim
{
    public class YarımGünTatilConverter : IValueConverter
    {
        private readonly List<Tuple<string, int>> tatiller = new List<Tuple<string, int>>();

        public YarımGünTatilConverter() => tatiller.Add(new Tuple<string, int>("Ekim", 28));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is Data data && tatiller.Any(z => z.Item1 == data.Ay && z.Item2 == data.Gün);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
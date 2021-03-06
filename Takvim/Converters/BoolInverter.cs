﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Takvim
{
    public class BoolInverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool x ? !x : value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Convert(value, targetType, parameter, culture);

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
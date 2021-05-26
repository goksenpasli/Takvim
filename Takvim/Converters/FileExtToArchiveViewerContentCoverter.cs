﻿using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Takvim
{
    public class FileExtToArchiveViewerContentCoverter : IValueConverter
    {
        private string[] SupportedExtensions { get; } = new string[] { ".zip", ".tar", ".rar" };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is string extension && SupportedExtensions.Contains(extension.ToLower());

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
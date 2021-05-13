using SharpCompress.Readers;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;

namespace Takvim
{
    public class FilePathToArchiveContentCoverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string yol)
            {
                try
                {
                    ObservableCollection<ArchiveData> Arşivİçerik = new();
                    using Stream stream = File.OpenRead(yol);
                    using IReader reader = ReaderFactory.Open(stream);
                    string extractpath = $@"{Path.GetDirectoryName(yol)}\{Path.GetFileNameWithoutExtension(yol)}";
                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            Arşivİçerik.Add(new ArchiveData() { DosyaAdı = reader.Entry.Key, Boyut = reader.Entry.Size, Crc = reader.Entry.Crc });
                        }
                    }
                    return Arşivİçerik;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
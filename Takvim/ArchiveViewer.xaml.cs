using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Takvim
{
    /// <summary>
    /// Interaction logic for ArchiveViewer.xaml
    /// </summary>
    public partial class ArchiveViewer : UserControl
    {
        public static readonly DependencyProperty ArchivePathProperty = DependencyProperty.Register("ArchivePath", typeof(string), typeof(ArchiveViewer), new PropertyMetadata(null));

        public ArchiveViewer()
        {
            InitializeComponent();
            DataContext=this;

            ArşivTekDosyaÇıkar = new RelayCommand<object>(parameter =>
            {
                try
                {
                    string seçilidosya = parameter as string;
                    using Stream stream = File.OpenRead(ArchivePath);
                    using IReader reader = ReaderFactory.Open(stream);
                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory && reader.Entry.Key == seçilidosya)
                        {
                            reader.WriteEntryToDirectory(Path.GetTempPath(), new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                    Process.Start($@"{Path.GetTempPath()}\{seçilidosya}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Dosya Açılamadı.\n" + ex.Message, "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }, parameter => true);
        }

        public string ArchivePath
        {
            get => (string)GetValue(ArchivePathProperty);
            set => SetValue(ArchivePathProperty, value);
        }

        public ICommand ArşivTekDosyaÇıkar { get; }
    }
}
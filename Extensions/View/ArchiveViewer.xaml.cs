using Extensions;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Extensions
{
    /// <summary>
    /// Interaction logic for ArchiveViewer.xaml
    /// </summary>
    public partial class ArchiveViewer : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ArchivePathProperty = DependencyProperty.Register("ArchivePath", typeof(string), typeof(ArchiveViewer), new PropertyMetadata(null));

        private static double toplamOran;

        private string aramaMetni;

        public ArchiveViewer()
        {
            InitializeComponent();
            Grid.DataContext = this;
            Cvs = Grid.TryFindResource("Cvs") as CollectionViewSource;

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
                            break;
                        }
                    }
                    Process.Start($@"{Path.GetTempPath()}\{seçilidosya}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Dosya Açılamadı.\n" + ex.Message, "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }, parameter => true);

            PropertyChanged += ArchiveViewer_PropertyChanged;
        }

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public static double ToplamOran
        {
            get => toplamOran;

            set
            {
                toplamOran = value;
                StaticPropertyChanged?.Invoke(null, new(nameof(ToplamOran)));
            }
        }

        public string AramaMetni
        {
            get => aramaMetni;

            set
            {
                if (aramaMetni != value)
                {
                    aramaMetni = value;
                    OnPropertyChanged(nameof(AramaMetni));
                }
            }
        }

        public string ArchivePath
        {
            get => (string)GetValue(ArchivePathProperty);
            set => SetValue(ArchivePathProperty, value);
        }

        public ICommand ArşivTekDosyaÇıkar { get; }

        private CollectionViewSource Cvs { get; set; }

        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ArchiveViewer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AramaMetni")
            {
                if (string.IsNullOrWhiteSpace(AramaMetni) && Cvs.View is not null)
                {
                    Cvs.View.Filter = null;
                }
                else
                {
                    Cvs.Filter += (s, e) => e.Accepted = (e.Item as ArchiveData)?.DosyaAdı.Contains(AramaMetni, StringComparison.OrdinalIgnoreCase) ?? false;
                }
            }
        }
    }
}
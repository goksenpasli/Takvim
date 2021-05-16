using Microsoft.Win32;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Takvim.Properties;

namespace Takvim
{
    public class CompressorViewModel : InpcBase
    {
        private CompressorView zipView;

        private Visibility textBlockVisible;

        public CompressorViewModel()
        {
            CompressorView = new CompressorView();
            DosyaKaydet = new RelayCommand<object>(parameter =>
            {
                SaveFileDialog saveFileDialog = new() { Filter = "Zip Dosyası (*.zip)|*.zip|Tar Dosyası (*.tar)|*.tar", AddExtension = true, Title = "Kaydet" };
                switch (CompressorView.Biçim)
                {
                    case 0:
                    case 2:
                        {
                            saveFileDialog.FilterIndex = 1;
                            break;
                        }
                    case 1:
                        {
                            saveFileDialog.FilterIndex = 2;
                            break;
                        }
                }
                if (saveFileDialog.ShowDialog() == true)
                {
                    CompressorView.KayıtYolu = saveFileDialog.FileName;
                }
            }, parameter => true);

            ArşivlenecekDosyalarıSeç = new RelayCommand<object>(parameter =>
            {
                OpenFileDialog openFileDialog = new() { Multiselect = true, Title = "Dosya Seç", Filter = " Tüm Dosyalar (*.*)|*.*" };
                if (openFileDialog.ShowDialog() == true)
                {
                    CompressorView.Dosyalar = new ObservableCollection<string>();
                    foreach (string filename in openFileDialog.FileNames)
                    {
                        CompressorView.Dosyalar.Add(filename);
                    }
                }
            }, parameter => true);

            ArşivTümünüÇıkar = new RelayCommand<object>(parameter =>
            {
                OpenFileDialog openFileDialog = new() { Multiselect = false, Title = "Dosya Seç", Filter = "Arşiv Dosyası (*.zip;*.tar;*.gzip;*.rar;*.xz;*.tar.gz;*.tar.bz2;*.tar.lz;*.tar.xz)|*.zip;*.tar;*.gzip;*.rar;*.xz;*.tar.gz;*.tar.bz2;*.tar.lz;*.tar.xz" };
                if (openFileDialog.ShowDialog() == true)
                {
                    ArşivTümünüAyıkla(openFileDialog.FileName);
                }
            }, parameter => true);

            ArşivİçerikGör = new RelayCommand<object>(parameter =>
            {
                OpenFileDialog openFileDialog = new() { Multiselect = false, Title = "Dosya Seç", Filter = "Arşiv Dosyası (*.zip;*.tar;*.gzip;*.rar;*.xz;*.tar.gz;*.tar.bz2;*.tar.lz;*.tar.xz)|*.zip;*.tar;*.gzip;*.rar;*.xz;*.tar.gz;*.tar.bz2;*.tar.lz;*.tar.xz" };
                if (openFileDialog.ShowDialog() == true)
                {
                    CompressorView.ArşivDosyaYolu = openFileDialog.FileName;
                }
            }, parameter => true);

            ArşivAç = new RelayCommand<object>(parameter => ArşivTümünüAyıkla(parameter as string), parameter => !string.IsNullOrWhiteSpace(parameter as string));

            ListedenDosyaSil = new RelayCommand<object>(parameter =>
            {
                if (parameter is string dosya && CompressorView?.Dosyalar.Count > 0)
                {
                    CompressorView.Dosyalar.Remove(dosya);
                }
            }, parameter => true);

            Arşivle = new RelayCommand<object>(parameter =>
            {
                Task.Factory.StartNew(() =>
                {
                    CompressorView.Sürüyor = true;
                    switch (CompressorView.Biçim)
                    {
                        case 0:
                            {
                                using FileStream zip = File.OpenWrite(CompressorView.KayıtYolu);
                                ZipWriterOptions zipWriterOptions = new(CompressionType.Deflate) { UseZip64 = true, DeflateCompressionLevel = (SharpCompress.Compressors.Deflate.CompressionLevel)CompressorView.SıkıştırmaDerecesi };
                                using ZipWriter zipWriter = new(zip, zipWriterOptions);

                                foreach (string dosya in CompressorView.Dosyalar)
                                {
                                    zipWriter.Write(Path.GetFileName(dosya), dosya);
                                    CompressorView.Oran++;
                                    CompressorView.DosyaAdı = Path.GetFileName(dosya);
                                }
                                break;
                            }

                        case 1:
                            {
                                using FileStream tar = File.OpenWrite(CompressorView.KayıtYolu);
                                using IWriter tarWriter = WriterFactory.Open(tar, ArchiveType.Tar, CompressionType.None);
                                foreach (string dosya in CompressorView.Dosyalar)
                                {
                                    tarWriter.Write(Path.GetFileName(dosya), dosya);
                                    CompressorView.Oran++;
                                    CompressorView.DosyaAdı = Path.GetFileName(dosya);
                                }
                                break;
                            }

                        case 2:
                            {
                                using FileStream zip = File.OpenWrite(CompressorView.KayıtYolu);
                                using IWriter zipWriter = WriterFactory.Open(zip, ArchiveType.Zip, CompressionType.LZMA);
                                foreach (string dosya in CompressorView.Dosyalar)
                                {
                                    zipWriter.Write(Path.GetFileName(dosya), dosya);
                                    CompressorView.Oran++;
                                    CompressorView.DosyaAdı = Path.GetFileName(dosya);
                                }
                                break;
                            }
                    }

                    CompressorView.Sürüyor = false;
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).ContinueWith(_ =>
                {
                    Settings.Default.SonArşivKayıtYeri.Add($@"{Path.GetDirectoryName(CompressorView.KayıtYolu)}\{Path.GetFileNameWithoutExtension(CompressorView.KayıtYolu)}");
                    Settings.Default.Save();
                    CompressorView.Dosyalar.Clear();
                    CompressorView.VeriGirişKayıtYolu = CompressorView.KayıtYolu;
                    CompressorView.KayıtYolu = null;
                    CompressorView.Oran = 0;
                    CompressorView.DosyaAdı = null;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }, parameter => true);

            CompressorView.PropertyChanged += ZipView_PropertyChanged;
        }

        public ICommand ArşivAç { get; }

        public ICommand ArşivİçerikGör { get; }

        public ICommand Arşivle { get; }

        public ICommand ArşivlenecekDosyalarıSeç { get; }

        public ICommand ArşivTekDosyaÇıkar { get; }

        public ICommand ArşivTümünüÇıkar { get; }

        public CompressorView CompressorView
        {
            get => zipView;

            set
            {
                if (zipView != value)
                {
                    zipView = value;
                    OnPropertyChanged(nameof(CompressorView));
                }
            }
        }

        public Visibility TextBlockVisible
        {
            get => textBlockVisible;

            set
            {
                if (textBlockVisible != value)
                {
                    textBlockVisible = value;
                    OnPropertyChanged(nameof(TextBlockVisible));
                }
            }
        }

        public ICommand DosyaKaydet { get; }

        public ICommand ListedenDosyaSil { get; }

        private void ArşivTümünüAyıkla(string yol, bool klasörgöster = true)
        {
            try
            {
                using Stream stream = File.OpenRead(yol);
                using IReader reader = ReaderFactory.Open(stream);
                string extractpath = $@"{Path.GetDirectoryName(yol)}\{Path.GetFileNameWithoutExtension(yol)}";
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        reader.WriteEntryToDirectory(extractpath, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
                if (klasörgöster)
                {
                    Process.Start(extractpath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dosya Arşiv Dosyası Olarak Okunanmadı.\n\n" + ex.Message, "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void ZipView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Biçim")
            {
                if ((CompressorView.Biçim == 0 || CompressorView.Biçim == 2) && CompressorView.KayıtYolu?.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) == false)
                {
                    CompressorView.KayıtYolu = Path.ChangeExtension(CompressorView.KayıtYolu, ".zip");
                }

                if ((CompressorView.Biçim == 1) && CompressorView.KayıtYolu?.EndsWith(".tar", StringComparison.OrdinalIgnoreCase) == false)
                {
                    CompressorView.KayıtYolu = Path.ChangeExtension(CompressorView.KayıtYolu, ".tar");
                }
            }
        }
    }
}
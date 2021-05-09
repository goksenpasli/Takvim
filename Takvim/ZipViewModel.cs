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
    public class ZipViewModel : InpcBase
    {
        private ZipView zipView;

        public ZipView ZipView
        {
            get => zipView;

            set
            {
                if (zipView != value)
                {
                    zipView = value;
                    OnPropertyChanged(nameof(ZipView));
                }
            }
        }

        public ZipViewModel()
        {
            ZipView = new ZipView();
            DosyaKaydet = new RelayCommand<object>(parameter =>
            {
                SaveFileDialog saveFileDialog = new() { Filter = "Zip Dosyası (*.zip)|*.zip|Tar Dosyası (*.tar)|*.tar", AddExtension = true, Title = "Kaydet" };
                switch (ZipView.Biçim)
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
                    ZipView.KayıtYolu = saveFileDialog.FileName;
                }
            }, parameter => true);

            DosyaAç = new RelayCommand<object>(parameter =>
            {
                OpenFileDialog openFileDialog = new() { Multiselect = true, Title = "Dosya Seç", Filter = " Tüm Dosyalar (*.*)|*.*" };
                if (openFileDialog.ShowDialog() == true)
                {
                    ZipView.Dosyalar = new ObservableCollection<string>();
                    foreach (string filename in openFileDialog.FileNames)
                    {
                        ZipView.Dosyalar.Add(filename);
                    }
                }
            }, parameter => true);

            ArşivÇıkart = new RelayCommand<object>(parameter =>
            {
                OpenFileDialog openFileDialog = new() { Multiselect = false, Title = "Dosya Seç", Filter = "Arşiv Dosyası (*.zip;*.tar;*.gzip;*.rar;*.xz)|*.zip;*.tar;*.gzip;*.rar;*.xz" };
                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        using Stream stream = File.OpenRead(openFileDialog.FileName);
                        using IReader reader = ReaderFactory.Open(stream);
                        string extractpath = $"{Path.GetDirectoryName(openFileDialog.FileName)}\\{Path.GetFileNameWithoutExtension(openFileDialog.FileName)}";
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
                        Process.Start(extractpath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }, parameter => true);

            ListedenDosyaSil = new RelayCommand<object>(parameter =>
            {
                if (parameter is string dosya && ZipView?.Dosyalar.Count>0)
                {
                    ZipView.Dosyalar.Remove(dosya);
                }
            }, parameter => true);

            ZipArşivle = new RelayCommand<object>(parameter =>
            {
                Task.Factory.StartNew(() =>
                {
                    ZipView.Sürüyor = true;
                    switch (ZipView.Biçim)
                    {
                        case 0:
                            {
                                using FileStream zip = File.OpenWrite(ZipView.KayıtYolu);
                                ZipWriterOptions zipWriterOptions = new(CompressionType.Deflate) { DeflateCompressionLevel = (SharpCompress.Compressors.Deflate.CompressionLevel)ZipView.SıkıştırmaDerecesi };
                                using ZipWriter zipWriter = new(zip, zipWriterOptions);

                                foreach (string dosya in ZipView.Dosyalar)
                                {
                                    zipWriter.Write(Path.GetFileName(dosya), dosya);
                                    ZipView.Oran++;
                                    ZipView.DosyaAdı = Path.GetFileName(dosya);
                                }
                                break;
                            }

                        case 1:
                            {
                                using FileStream tar = File.OpenWrite(ZipView.KayıtYolu);
                                using IWriter tarWriter = WriterFactory.Open(tar, ArchiveType.Tar, CompressionType.None);
                                foreach (string dosya in ZipView.Dosyalar)
                                {
                                    tarWriter.Write(Path.GetFileName(dosya), dosya);
                                    ZipView.Oran++;
                                    ZipView.DosyaAdı = Path.GetFileName(dosya);
                                }
                                break;
                            }

                        case 2:
                            {
                                using FileStream zip = File.OpenWrite(ZipView.KayıtYolu);
                                using IWriter zipWriter = WriterFactory.Open(zip, ArchiveType.Zip, CompressionType.LZMA);
                                foreach (string dosya in ZipView.Dosyalar)
                                {
                                    zipWriter.Write(Path.GetFileName(dosya), dosya);
                                    ZipView.Oran++;
                                    ZipView.DosyaAdı = Path.GetFileName(dosya);
                                }
                                break;
                            }
                    }

                    ZipView.Sürüyor = false;
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).ContinueWith(_ =>
                {
                    Settings.Default.SonArşivKayıtYeri.Add(ZipView.KayıtYolu);
                    Settings.Default.Save();
                    ZipView.Dosyalar.Clear();
                    ZipView.KayıtYolu = null;
                    ZipView.Oran = 0;
                    ZipView.DosyaAdı = null;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }, parameter => true);

            ZipView.PropertyChanged += ZipView_PropertyChanged;
        }

        private void ZipView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Biçim")
            {
                if ((ZipView.Biçim == 0 || ZipView.Biçim == 2) && ZipView.KayıtYolu?.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) == false)
                {
                    ZipView.KayıtYolu = Path.ChangeExtension(ZipView.KayıtYolu, ".zip");
                }

                if ((ZipView.Biçim == 1) && ZipView.KayıtYolu?.EndsWith(".tar", StringComparison.OrdinalIgnoreCase) == false)
                {
                    ZipView.KayıtYolu = Path.ChangeExtension(ZipView.KayıtYolu, ".tar");
                }
            }
        }

        public ICommand DosyaKaydet { get; }

        public ICommand ListedenDosyaSil { get; }

        public ICommand DosyaAç { get; }

        public ICommand ArşivÇıkart { get; }

        public ICommand ZipArşivle { get; }
    }
}

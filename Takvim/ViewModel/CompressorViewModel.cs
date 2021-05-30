using Extensions;
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Takvim
{
    public class CompressorViewModel : InpcBase
    {
        private readonly string[] imageextension = new string[] { ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff" };

        private Visibility elementVisible;

        private CompressorView zipView;

        public CompressorViewModel()
        {
            CompressorView = new CompressorView();
            DosyaKaydet = new RelayCommand<object>(parameter =>
            {
                SaveFileDialog saveFileDialog = new() { Filter = "Zip Dosyası (*.zip)|*.zip|Tar Dosyası (*.tar)|*.tar|Tgz Dosyası (*.tgz)|*.tgz", AddExtension = true, Title = "Kaydet" };
                switch (CompressorView.Biçim)
                {
                    case 0:
                    case 2:
                        saveFileDialog.FilterIndex = 1;
                        break;

                    case 1:
                        saveFileDialog.FilterIndex = 2;
                        break;

                    case 3:
                        saveFileDialog.FilterIndex = 3;
                        break;
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
                OpenFileDialog openFileDialog = new() { Multiselect = false, Title = "Dosya Seç", Filter = "Arşiv Dosyası (*.zip;*.tar;*.gzip;*.rar;*.xz;*.tar.gz;*.tar.bz2;*.tar.lz;*.tar.xz;*.tgz)|*.zip;*.tar;*.gzip;*.rar;*.xz;*.tar.gz;*.tar.bz2;*.tar.lz;*.tar.xz;*.tgz" };
                if (openFileDialog.ShowDialog() == true)
                {
                    ArşivTümünüAyıkla(openFileDialog.FileName);
                }
            }, parameter => true);

            ArşivİçerikGör = new RelayCommand<object>(parameter =>
            {
                OpenFileDialog openFileDialog = new() { Multiselect = false, Title = "Dosya Seç", Filter = "Arşiv Dosyası (*.zip;*.tar;*.gzip;*.rar;*.xz;*.tar.gz;*.tar.bz2;*.tar.lz;*.tar.xz;*.tgz)|*.zip;*.tar;*.gzip;*.rar;*.xz;*.tar.gz;*.tar.bz2;*.tar.lz;*.tar.xz;*.tgz" };
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
                      try
                      {
                          CompressorView.Sürüyor = true;
                          using FileStream stream = File.OpenWrite(CompressorView.KayıtYolu);
                          switch (CompressorView.Biçim)
                          {
                              case 0:
                                  {
                                      using ZipWriter writer = new(stream, new ZipWriterOptions(CompressionType.Deflate) { UseZip64 = true, DeflateCompressionLevel = (SharpCompress.Compressors.Deflate.CompressionLevel)CompressorView.SıkıştırmaDerecesi });
                                      WriteData(writer);
                                      break;
                                  }

                              case 1:
                                  {
                                      using IWriter writer = WriterFactory.Open(stream, ArchiveType.Tar, CompressionType.None);
                                      WriteData(writer);
                                      break;
                                  }

                              case 2:
                                  {
                                      using IWriter writer = WriterFactory.Open(stream, ArchiveType.Zip, CompressionType.LZMA);
                                      WriteData(writer);
                                      break;
                                  }

                              case 3:
                                  {
                                      using IWriter writer = WriterFactory.Open(stream, ArchiveType.Tar, CompressionType.GZip);
                                      WriteData(writer);
                                      break;
                                  }
                          }

                          CompressorView.Sürüyor = false;
                      }
                      catch (Exception Ex)
                      {
                          MessageBox.Show(Ex.Message, "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                      }
                  }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).ContinueWith(_ =>
                  {
                      CompressorView.Dosyalar.Clear();
                      CompressorView.VeriGirişKayıtYolu = CompressorView.KayıtYolu;
                      CompressorView.KayıtYolu = null;
                      CompressorView.Oran = 0;
                      CompressorView.DosyaAdı = null;
                  }, TaskScheduler.FromCurrentSynchronizationContext());
            }, parameter => CompressorView.Dosyalar?.Contains(CompressorView.KayıtYolu) != true);

            CompressorView.PropertyChanged += ZipView_PropertyChanged;

            void WriteWebpFile(string dosya, out string webpfilename, out string tempfile)
            {
                webpfilename = Path.GetFileNameWithoutExtension(dosya) + ".webp";
                tempfile = $@"{Path.GetTempPath()}\{webpfilename}";
                File.WriteAllBytes(tempfile, dosya.WebpEncode(CompressorView.WebpQuality));
            }

            void WriteData(IWriter writer)
            {
                foreach (string dosya in CompressorView.Dosyalar)
                {
                    if (CompressorView.ResimleriWebpZiple && imageextension.Contains(Path.GetExtension(dosya).ToLower()))
                    {
                        WriteWebpFile(dosya, out string webpfilename, out string tempfile);
                        writer.Write(Path.GetFileName(webpfilename), tempfile);
                        File.Delete(tempfile);
                    }
                    else
                    {
                        writer.Write(Path.GetFileName(dosya), dosya);
                    }
                    CompressorView.Oran++;
                    CompressorView.DosyaAdı = Path.GetFileName(dosya);
                }
            }
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

        public ICommand DosyaKaydet { get; }

        public Visibility ElementVisible
        {
            get => elementVisible;

            set
            {
                if (elementVisible != value)
                {
                    elementVisible = value;
                    OnPropertyChanged(nameof(ElementVisible));
                }
            }
        }

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
            if (e.PropertyName == "ResimleriWebpZiple" && CompressorView.ResimleriWebpZiple && CompressorView.Dosyalar?.All(z => imageextension.Contains(Path.GetExtension(z))) == true)
            {
                CompressorView.Biçim = 1;
            }

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
                if ((CompressorView.Biçim == 3) && CompressorView.KayıtYolu?.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase) == false)
                {
                    CompressorView.KayıtYolu = Path.ChangeExtension(CompressorView.KayıtYolu, ".tgz");
                }
            }
        }
    }
}
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace Takvim
{
    public class Data : InpcBase, IDisposable
    {
        private int gün;

        private string günAdı;

        private string ay;

        private int offset;

        private string günNotAçıklama;

        private DateTime tamTarih;

        private byte[] resimData;

        private bool önemliMi;

        private string resimUzantı;

        private int veriSayısı;

        private Brush veriRenk;

        private double etkinlikSüresi;

        private ObservableCollection<string> dosyalar;

        private int ıd;

        public int Gün
        {
            get => gün;

            set
            {
                if (gün != value)
                {
                    gün = value;
                    OnPropertyChanged(nameof(Gün));
                }
            }
        }

        public string Ay
        {
            get => ay;

            set
            {
                if (ay != value)
                {
                    ay = value;
                    OnPropertyChanged(nameof(Ay));
                }
            }
        }

        public string GünAdı
        {
            get => günAdı;

            set
            {
                if (günAdı != value)
                {
                    günAdı = value;
                    OnPropertyChanged(nameof(GünAdı));
                }
            }
        }

        public DateTime TamTarih
        {
            get { return tamTarih; }

            set
            {
                if (tamTarih != value)
                {
                    tamTarih = value;
                    OnPropertyChanged(nameof(TamTarih));
                }
            }
        }

        public int Offset
        {
            get => offset;

            set
            {
                if (offset != value)
                {
                    offset = value;
                    OnPropertyChanged(nameof(Offset));
                }
            }
        }

        public string GünNotAçıklama
        {
            get => günNotAçıklama;

            set
            {
                if (günNotAçıklama != value)
                {
                    günNotAçıklama = value;
                    OnPropertyChanged(nameof(GünNotAçıklama));
                }
            }
        }

        public int Id
        {
            get => ıd;

            set
            {
                if (ıd != value)
                {
                    ıd = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public byte[] ResimData
        {
            get { return resimData; }

            set
            {
                if (resimData != value)
                {
                    resimData = value;
                    OnPropertyChanged(nameof(ResimData));
                }
            }
        }

        public bool ÖnemliMi
        {
            get => önemliMi;

            set
            {
                if (önemliMi != value)
                {
                    önemliMi = value;
                    OnPropertyChanged(nameof(ÖnemliMi));
                }
            }
        }

        public string ResimUzantı
        {
            get => resimUzantı;

            set
            {
                if (resimUzantı != value)
                {
                    resimUzantı = value;
                    OnPropertyChanged(nameof(ResimUzantı));
                }
            }
        }

        public int VeriSayısı
        {
            get => veriSayısı;

            set
            {
                if (veriSayısı != value)
                {
                    veriSayısı = value;
                    OnPropertyChanged(nameof(VeriSayısı));
                }
            }
        }

        public Brush VeriRenk
        {
            get => veriRenk;

            set
            {
                if (veriRenk != value)
                {
                    veriRenk = value;
                    OnPropertyChanged(nameof(VeriRenk));
                }
            }
        }

        public double EtkinlikSüresi
        {
            get => etkinlikSüresi;

            set
            {
                if (etkinlikSüresi != value)
                {
                    etkinlikSüresi = value;
                    OnPropertyChanged(nameof(EtkinlikSüresi));
                }
            }
        }

        public ObservableCollection<string> Dosyalar
        {
            get => dosyalar;

            set
            {
                if (dosyalar != value)
                {
                    dosyalar = value;
                    OnPropertyChanged(nameof(Dosyalar));
                }
            }
        }

        public Data()
        {
            Window verigirişwindow = null;

            XmlVeriEkle = new RelayCommand(parameter =>
            {
                WriteXmlRootData(MainViewModel.xmlpath);
                var xDocument = XDocument.Load(MainViewModel.xmlpath);
                var parentElement = new XElement("Veri");

                parentElement.Add(new XAttribute("Id", new Random().Next(1, int.MaxValue)));
                parentElement.Add(new XAttribute("Onemli", ÖnemliMi));
                parentElement.Add(new XAttribute("Saat", EtkinlikSüresi));
                if (VeriRenk != null)
                {
                    parentElement.Add(new XAttribute("Renk", VeriRenk));
                }

                var xmlcontent = new object[3];
                xmlcontent[0] = new XElement("Gun", TamTarih);
                xmlcontent[1] = new XElement("Aciklama", GünNotAçıklama);
                if (ResimData != null && ResimUzantı != null)
                {
                    var xElement = new XElement("Resim", Convert.ToBase64String(ResimData));
                    xElement.Add(new XAttribute("Ext", ResimUzantı));
                    xmlcontent[2] = xElement;
                }

                var xmlfiles = new XElement("Dosyalar");
                if (Dosyalar != null)
                {
                    foreach (var dosya in Dosyalar)
                    {
                        var file = new XElement("Dosya");
                        file.Add(new XAttribute("Yol", dosya));
                        file.Add(new XAttribute("Ad", Path.GetFileNameWithoutExtension(dosya)));
                        file.Add(new XAttribute("Ext", Path.GetExtension(dosya)));
                        xmlfiles.Add(file);
                    }
                }

                parentElement.Add(xmlcontent);
                parentElement.Add(xmlfiles);
                xDocument.Element("Veriler")?.Add(parentElement);
                xDocument.Save(MainViewModel.xmlpath);
                VeriSayısı++;
                verigirişwindow.Close();
                MainViewModel.xmlDataProvider.Refresh();
                Dispose(true);
            }, parameter => !string.IsNullOrWhiteSpace(GünNotAçıklama));

            ResimYükle = new RelayCommand(parameter =>
            {
                const int filelimit = 100 * 1024;
                OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = false, Filter = "Resim Dosyaları (*.jpg;*.jpeg;*.tif;*.tiff)|*.jpg;*.jpeg;*.tif;*.tiff)" };
                if (openFileDialog.ShowDialog() == true)
                {
                    var data = File.ReadAllBytes(openFileDialog.FileName);
                    if (data.Length < filelimit)
                    {
                        ResimData = data;
                        ResimUzantı = Path.GetExtension(openFileDialog.FileName).ToLower();
                    }
                    else
                    {
                        MessageBox.Show($"Resim Boyutu En Çok {filelimit / 1024} KB Olabilir.", "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }, parameter => !string.IsNullOrWhiteSpace(GünNotAçıklama));

            DosyalarYükle = new RelayCommand(parameter =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = true, Filter = "Tüm Dosyalar (*.*)|*.*" };
                if (openFileDialog.ShowDialog() == true)
                {
                    Dosyalar = new ObservableCollection<string>();
                    foreach (var dosya in openFileDialog.FileNames)
                    {
                        Dosyalar.Add(dosya);
                    }
                }
            }, parameter => !string.IsNullOrWhiteSpace(GünNotAçıklama));

            ResimSakla = new RelayCommand(parameter =>
            {
                if (parameter is XmlElement xmlElement)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Title = "SAKLA",
                        Filter = "Resim Dosyaları (*.jpg;*.jpeg;*.tif;*.tiff)|*.jpg;*.jpeg;*.tif;*.tiff)",
                        FileName = xmlElement.PreviousSibling?.InnerText + xmlElement.GetAttribute("Ext")
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        var bytes = Convert.FromBase64String(xmlElement.InnerText);
                        using var imageFile = new FileStream(saveFileDialog.FileName, FileMode.Create);
                        imageFile.Write(bytes, 0, bytes.Length);
                        imageFile.Flush();
                    }
                }
            }, parameter => true);

            DosyaAç = new RelayCommand(parameter =>
            {
                if (parameter is XmlAttribute xmlAttribute)
                {
                    Process.Start(xmlAttribute.Value);
                }
            }, parameter => parameter is XmlAttribute xmlAttribute && File.Exists(xmlAttribute.Value));

            Dosyalarİptal = new RelayCommand(parameter => Dosyalar = null, parameter => Dosyalar?.Count > 0);

            Resimİptal = new RelayCommand(parameter => ResimData = null, parameter => ResimData?.Length > 0);

            VeriEkleEkranı = new RelayCommand(parameter =>
            {
                verigirişwindow = new Window
                {
                    Title = TamTarih.ToShortDateString(),
                    Content = new DataEnterWindow(),
                    DataContext = this,
                    Width = 300,
                    WindowStyle = WindowStyle.ToolWindow,
                    Height = 250,
                    Owner = Application.Current.MainWindow,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                verigirişwindow.ShowDialog();
            }, parameter => true);
        }

        private static void WriteXmlRootData(string xmlfilepath)
        {
            if (!File.Exists(xmlfilepath))
            {
                using XmlWriter writer = XmlWriter.Create(MainViewModel.xmlpath);
                writer.WriteStartElement("Veriler");
                writer.WriteEndElement();
                writer.Flush();
            }
        }

        public ICommand XmlVeriEkle { get; }

        public ICommand ResimYükle { get; }

        public ICommand DosyalarYükle { get; }

        public ICommand Dosyalarİptal { get; }

        public ICommand DosyaAç { get; }

        public ICommand Resimİptal { get; }

        public ICommand ResimSakla { get; }

        public ICommand VeriEkleEkranı { get; }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GünNotAçıklama = null;
                ResimData = null;
                Dosyalar = null;
                VeriRenk = null;
                ÖnemliMi = false;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
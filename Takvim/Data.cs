using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace Takvim
{
    public class Data : InpcBase, IDisposable
    {
        private string ay;

        private ObservableCollection<string> dosyalar;

        private double etkinlikSüresi;

        private short gün;

        private string günAdı;

        private string günNotAçıklama;

        private int ıd;

        private short offset;

        private bool önemliMi;

        private byte[] resimData;

        private string resimUzantı;

        private string saatBaşlangıç;

        private DateTime tamTarih;

        private Brush veriRenk;

        private int veriSayısı;

        public Data()
        {
            Window verigirişwindow = null;

            XmlVeriEkle = new RelayCommand(parameter =>
            {
                WriteXmlRootData(MainViewModel.xmlpath);
                XDocument xDocument = XDocument.Load(MainViewModel.xmlpath);
                XElement parentElement = new XElement("Veri");

                parentElement.Add(new XAttribute("Id", new Random().Next(1, int.MaxValue)));
                parentElement.Add(new XAttribute("Onemli", ÖnemliMi));
                parentElement.Add(new XAttribute("Saat", EtkinlikSüresi));
                parentElement.Add(new XAttribute("SaatBaslangic", SaatBaşlangıç));
                if (VeriRenk != null)
                {
                    parentElement.Add(new XAttribute("Renk", VeriRenk));
                }

                object[] xmlcontent = new object[3];
                xmlcontent[0] = new XElement("Gun", TamTarih);
                xmlcontent[1] = new XElement("Aciklama", GünNotAçıklama);
                if (ResimData != null && ResimUzantı != null)
                {
                    XElement xElement = new XElement("Resim", Convert.ToBase64String(ResimData));
                    xElement.Add(new XAttribute("Ext", ResimUzantı));
                    xmlcontent[2] = xElement;
                }

                XElement xmlfiles = new XElement("Dosyalar");
                if (Dosyalar != null)
                {
                    foreach (string dosya in Dosyalar)
                    {
                        XElement file = new XElement("Dosya");
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
            }, parameter => !string.IsNullOrWhiteSpace(GünNotAçıklama) && DateTime.TryParseExact(SaatBaşlangıç, "H:m", new CultureInfo("tr-TR"), DateTimeStyles.None, out _));

            ResimYükle = new RelayCommand(parameter =>
            {
                const int filelimit = 100 * 1024;
                OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = false, Filter = "Resim Dosyaları (*.jpg;*.jpeg;*.tif;*.tiff)|*.jpg;*.jpeg;*.tif;*.tiff)" };
                if (openFileDialog.ShowDialog() == true)
                {
                    byte[] data = File.ReadAllBytes(openFileDialog.FileName);
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
                    foreach (string dosya in openFileDialog.FileNames)
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
                        byte[] bytes = Convert.FromBase64String(xmlElement.InnerText);
                        using FileStream imageFile = new FileStream(saveFileDialog.FileName, FileMode.Create);
                        imageFile.Write(bytes, 0, bytes.Length);
                        imageFile.Flush();
                    }
                }
            }, parameter => true);


            PencereKapat = new RelayCommand(parameter =>
            {
                if (parameter is Window window)
                {
                    window.Close();
                }
            }, parameter => true);

            DosyaAç = new RelayCommand(parameter =>
            {
                if (parameter is XmlAttribute xmlAttribute)
                {
                    Process.Start(xmlAttribute.Value);
                }
            }, parameter => parameter is XmlAttribute xmlAttribute && File.Exists(xmlAttribute.Value));

            XmlVeriSil = new RelayCommand(parameter =>
            {
                if (parameter is XmlAttribute Id && MessageBox.Show("Seçili kaydı silmek istiyor musun?", "TAKVİM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    XDocument doc = XDocument.Load(MainViewModel.xmlpath);
                    doc.Root.Elements("Veri").Where(z => z.Attribute("Id").Value == Id.Value).Remove();
                    doc.Save(MainViewModel.xmlpath);
                    MainViewModel.xmlDataProvider.Refresh();
                    VeriSayısı--;
                }
            }, parameter => true);

            Dosyalarİptal = new RelayCommand(parameter => Dosyalar = null, parameter => Dosyalar?.Count > 0);

            Resimİptal = new RelayCommand(parameter => ResimData = null, parameter => ResimData?.Length > 0);

            VeriEkleEkranı = new RelayCommand(parameter =>
            {
                verigirişwindow = new Window
                {
                    Title = TamTarih.ToString("dd MMMM yyyy dddd"),
                    Content = new DataEnterWindow(),
                    DataContext = this,
                    Width = 450,
                    AllowsTransparency = true,
                    WindowStyle = WindowStyle.None,
                    Height = 300,
                    Background = Brushes.Transparent,
                    Owner = Application.Current.MainWindow,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                verigirişwindow.MouseLeftButtonDown += (s, e) => verigirişwindow.DragMove();
                verigirişwindow.ShowDialog();
            }, parameter => true);
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

        public ICommand DosyaAç { get; }

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

        public ICommand Dosyalarİptal { get; }

        public ICommand DosyalarYükle { get; }

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

        public short Gün
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

        public short Offset
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

        public ICommand PencereKapat { get; }

        public byte[] ResimData
        {
            get => resimData;

            set
            {
                if (resimData != value)
                {
                    resimData = value;
                    OnPropertyChanged(nameof(ResimData));
                }
            }
        }

        public ICommand Resimİptal { get; }

        public ICommand ResimSakla { get; }

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

        public ICommand ResimYükle { get; }

        public string SaatBaşlangıç
        {
            get => saatBaşlangıç;

            set
            {
                if (saatBaşlangıç != value)
                {
                    saatBaşlangıç = value;
                    OnPropertyChanged(nameof(SaatBaşlangıç));
                }
            }
        }

        public DateTime TamTarih
        {
            get => tamTarih;

            set
            {
                if (tamTarih != value)
                {
                    tamTarih = value;
                    OnPropertyChanged(nameof(TamTarih));
                }
            }
        }

        public ICommand VeriEkleEkranı { get; }

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

        public ICommand XmlVeriEkle { get; }

        public ICommand XmlVeriSil { get; }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

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

        private static void WriteXmlRootData(string xmlfilepath)
        {
            if (!Directory.Exists(MainViewModel.xmldatasavefolder))
            {
                Directory.CreateDirectory(MainViewModel.xmldatasavefolder);
            }
            if (!File.Exists(xmlfilepath))
            {
                using XmlWriter writer = XmlWriter.Create(MainViewModel.xmlpath);
                writer.WriteStartElement("Veriler");
                writer.WriteEndElement();
                writer.Flush();
            }
        }
    }
}
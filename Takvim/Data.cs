﻿using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace Takvim
{
    public class Data : InpcBase
    {
        private string ay;

        private bool ayTekrar;

        private double boyut;

        private ObservableCollection<string> dosyalar;

        private double etkinlikSüresi;

        private short gün;

        private string günAdı;

        private string günNotAçıklama;

        private int ıd;

        private string ocrMetin;

        private bool ocrSürüyor;

        private Task OcrTask;

        private short offset;

        private bool önemliMi;

        private byte[] resimData;

        private string resimUzantı;

        private string saatBaşlangıç;

        private DateTime tamTarih;

        private Brush veriRenk;

        private int veriSayısı;

        private int webpQuality = 20;

        private ObservableCollection<string> saatler=new ObservableCollection<string>();

        public Data()
        {
            Window verigirişwindow = null;

            XmlVeriEkle = new RelayCommand<object>(parameter =>
            {
                if (OcrTask?.Status != TaskStatus.Running)
                {
                    WriteXmlRootData(MainViewModel.xmlpath);
                    XDocument xDocument = XDocument.Load(MainViewModel.xmlpath);
                    XElement parentElement = new XElement("Veri");
                    parentElement.Add(new XAttribute("Id", new Random().Next(1, int.MaxValue)));
                    parentElement.Add(new XAttribute("Saat", EtkinlikSüresi));
                    parentElement.Add(new XAttribute("SaatBaslangic", SaatBaşlangıç));
                    parentElement.Add(new XAttribute("AyTekrar", AyTekrar.ToString().ToLower()));

                    if (VeriRenk != null)
                    {
                        parentElement.Add(new XAttribute("Renk", VeriRenk));
                    }
                    if (ÖnemliMi)
                    {
                        parentElement.Add(new XAttribute("Onemli", ÖnemliMi.ToString().ToLower()));
                    }

                    if (OcrMetin != null)
                    {
                        parentElement.Add(new XAttribute("Ocr", OcrMetin));
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

                    if (Dosyalar != null)
                    {
                        XElement xmlfiles = WriteFileElements(Dosyalar);
                        parentElement.Add(xmlfiles);
                    }

                    parentElement.Add(xmlcontent);
                    xDocument.Element("Veriler")?.Add(parentElement);
                    xDocument.Save(MainViewModel.xmlpath);
                    VeriSayısı++;
                    verigirişwindow?.Close();
                    MainViewModel.xmlDataProvider.Refresh();
                    CollectionViewSource.GetDefaultView((Application.Current.MainWindow.DataContext as MainViewModel)?.AyGünler).Refresh();
                }
                else
                {
                    MessageBox.Show("Ocr İşlemi Sürüyor Bitmesini Bekleyin.", "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }, parameter => !string.IsNullOrWhiteSpace(GünNotAçıklama) && DateTime.TryParseExact(SaatBaşlangıç, "H:m", new CultureInfo("tr-TR"), DateTimeStyles.None, out _));

            ResimYükle = new RelayCommand<object>(parameter =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = false, Filter = "Resim Dosyaları (*.jpg;*.jpeg;*.tif;*.tiff;*.png)|*.jpg;*.jpeg;*.tif;*.tiff;*.png" };
                if (openFileDialog.ShowDialog() == true)
                {
                    ResimData = openFileDialog.FileName.WebpEncode(WebpQuality);
                    ResimUzantı = ".webp";
                    Boyut = ResimData.Length / 1024;
                }
            }, parameter => Environment.OSVersion.Version.Major > 5);

            ResimSakla = new RelayCommand<object>(parameter =>
            {
                if (parameter is XmlElement xmlElement)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Title = "SAKLA",
                        Filter = "Resim Dosyaları (*.webp)|*.webp)",
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

            PencereKapat = new RelayCommand<object>(parameter =>
            {
                if (parameter is Window window)
                {
                    window.Close();
                }
            }, parameter => true);

            OcrUygula = new RelayCommand<object>(parameter =>
            {
                OcrTask = Task.Factory.StartNew(() =>
                {
                    OcrSürüyor = true;
                    OcrMetin = (parameter as byte[]).WebpDecode().ToTiffJpegByteArray(ExtensionMethods.Format.Jpg).OcrYap();
                    OcrSürüyor = false;
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            }, parameter => Environment.OSVersion.Version.Major > 5);

            DosyaAç = new RelayCommand<object>(parameter =>
            {
                if (parameter is XmlAttribute xmlAttribute)
                {
                    Process.Start(xmlAttribute.Value);
                }
            }, parameter => parameter is XmlAttribute xmlAttribute && File.Exists(xmlAttribute.Value));

            XmlVeriSil = new RelayCommand<object>(parameter =>
            {
                if (parameter is XmlAttribute Id && MessageBox.Show("Seçili kaydı silmek istiyor musun?", "TAKVİM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    XDocument doc = XDocument.Load(MainViewModel.xmlpath);
                    doc.Root.Elements("Veri").Where(z => z.Attribute("Id").Value == Id.Value).Remove();
                    doc.Save(MainViewModel.xmlpath);
                    MainViewModel.xmlDataProvider.Refresh();
                    CollectionViewSource.GetDefaultView((Application.Current.MainWindow.DataContext as MainViewModel)?.AyGünler).Refresh();
                    VeriSayısı--;
                }
            }, parameter => true);

            CsvDosyasınaYaz = new RelayCommand<object>(parameter =>
            {
                XDocument doc = XDocument.Load(MainViewModel.xmlpath);
                string dosyaismi = Path.GetTempPath() + Guid.NewGuid() + ".csv";
                string seperator = new CultureInfo(CultureInfo.CurrentCulture.Name).TextInfo.ListSeparator;
                foreach (XElement item in doc.Root.Elements("Veri"))
                {
                    File.AppendAllText(dosyaismi, $"{item.Element("Gun")?.Value}{seperator}{item.Attribute("SaatBaslangic")?.Value}{seperator}{item.Element("Aciklama")?.Value}\n", Encoding.UTF8);
                }
                Process.Start(dosyaismi);
            }, parameter => true);

            XmlVeriGüncelle = new RelayCommand<object>(parameter =>
            {
                if (parameter is XmlAttribute xmlattributeId)
                {
                    XDocument doc = XDocument.Load(MainViewModel.xmlpath);
                    UpdateAttribute(xmlattributeId, "SaatBaslangic", SaatBaşlangıç, doc);
                    UpdateAttribute(xmlattributeId, "Saat", EtkinlikSüresi.ToString(), doc);
                    UpdateAttribute(xmlattributeId, "AyTekrar", AyTekrar.ToString().ToLower(), doc);
                    doc.Save(MainViewModel.xmlpath);
                    MainViewModel.xmlDataProvider.Refresh();
                }
            }, parameter => DateTime.TryParseExact(SaatBaşlangıç, "H:m", new CultureInfo("tr-TR"), DateTimeStyles.None, out _));

            Resimİptal = new RelayCommand<object>(parameter => ResimData = null, parameter => ResimData?.Length > 0);

            VeriEkleEkranı = new RelayCommand<object>(parameter =>
            {
                verigirişwindow = new Window
                {
                    Title = TamTarih.ToString("dd MMMM yyyy dddd"),
                    Content = new DataEnterWindow(),
                    DataContext = this,
                    Width = 424,
                    AllowsTransparency = true,
                    WindowStyle = WindowStyle.None,
                    Height = 304,
                    Background = Brushes.Transparent,
                    Owner = Application.Current.MainWindow,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                Dosyalar = new ObservableCollection<string>();
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

        public bool AyTekrar
        {
            get => ayTekrar;

            set
            {
                if (ayTekrar != value)
                {
                    ayTekrar = value;
                    OnPropertyChanged(nameof(AyTekrar));
                }
            }
        }

        public double Boyut
        {
            get => boyut;

            set
            {
                if (boyut != value)
                {
                    boyut = value;
                    OnPropertyChanged(nameof(Boyut));
                }
            }
        }

        public ICommand CsvDosyasınaYaz { get; }

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

        public string OcrMetin
        {
            get => ocrMetin;

            set
            {
                if (ocrMetin != value)
                {
                    ocrMetin = value;
                    OnPropertyChanged(nameof(OcrMetin));
                }
            }
        }

        public bool OcrSürüyor
        {
            get => ocrSürüyor;

            set
            {
                if (ocrSürüyor != value)
                {
                    ocrSürüyor = value;
                    OnPropertyChanged(nameof(OcrSürüyor));
                }
            }
        }
        public ICommand OcrUygula { get; }

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

        public int WebpQuality
        {
            get => webpQuality;
            set
            {
                if (webpQuality != value)
                {
                    webpQuality = value;
                    OnPropertyChanged(nameof(WebpQuality));
                }
            }
        }

        public ObservableCollection<string> Saatler
        {
            get
            {
                for (DateTime i = DateTime.Today; i < DateTime.Today.AddDays(1); i = i.AddMinutes(30))
                {
                    saatler.Add(i.ToShortTimeString());
                }
                return saatler;
            }

            set
            {
                if (saatler != value)
                {
                    saatler = value;
                    OnPropertyChanged(nameof(Saatler));
                }
            }
        }

        public ICommand XmlVeriEkle { get; }

        public ICommand XmlVeriGüncelle { get; }

        public ICommand XmlVeriSil { get; }

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

        private XElement UpdateAttribute(XmlAttribute xmlAttribute, string attributevalue, string updatedattributevalue, XDocument doc)
        {
            XElement root = doc.Root.Elements("Veri").FirstOrDefault(z => z.Attribute("Id").Value == xmlAttribute.Value);
            root.Attribute(attributevalue).Value = updatedattributevalue;
            return root;
        }

        private XElement WriteFileElements(ObservableCollection<string> Dosyalar)
        {
            XElement xmlfiles = new XElement("Dosyalar");
            foreach (string dosya in Dosyalar)
            {
                XElement file = new XElement("Dosya");
                file.Add(new XAttribute("Yol", dosya));
                file.Add(new XAttribute("Ad", Path.GetFileNameWithoutExtension(dosya)));
                file.Add(new XAttribute("Ext", Path.GetExtension(dosya)));
                xmlfiles.Add(file);
            }

            return xmlfiles;
        }
    }
}
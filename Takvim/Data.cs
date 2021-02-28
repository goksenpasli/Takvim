using iCalNET.Model;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

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

        private bool kilitliMi;

        public Data()
        {
            Window verigirişwindow = null;

            XmlVeriEkle = new RelayCommand<object>(parameter =>
            {
                if (OcrTask?.Status != TaskStatus.Running)
                {
                    XmlDocument document = MainViewModel.xmlDataProvider.Document;
                    XmlNode rootNode = document.CreateElement("Veri");

                    XmlAttribute Id = document.CreateAttribute("Id");
                    Id.Value = new Random().Next(1, int.MaxValue).ToString();
                    XmlAttribute Saat = document.CreateAttribute("Saat");
                    Saat.Value = EtkinlikSüresi.ToString();
                    XmlAttribute SaatBaslangic = document.CreateAttribute("SaatBaslangic");
                    SaatBaslangic.Value = SaatBaşlangıç;
                    XmlAttribute Tekrar = document.CreateAttribute("AyTekrar");
                    Tekrar.Value = AyTekrar.ToString().ToLower();
                    rootNode.Attributes.Append(Id);
                    rootNode.Attributes.Append(Saat);
                    rootNode.Attributes.Append(SaatBaslangic);
                    rootNode.Attributes.Append(Tekrar);

                    XmlAttribute Renk = document.CreateAttribute("Renk");
                    Renk.Value = VeriRenk == null ? Brushes.Transparent.ToString() : VeriRenk.ToString();
                    rootNode.Attributes.Append(Renk);

                    if (ÖnemliMi)
                    {
                        XmlAttribute Onemli = document.CreateAttribute("Onemli");
                        Onemli.Value = ÖnemliMi.ToString().ToLower();
                        rootNode.Attributes.Append(Onemli);
                    }

                    if (KilitliMi)
                    {
                        XmlAttribute Kilitli = document.CreateAttribute("Kilitli");
                        Kilitli.Value = KilitliMi.ToString().ToLower();
                        rootNode.Attributes.Append(Kilitli);
                    }

                    XmlAttribute Okundu = document.CreateAttribute("Okundu");
                    Okundu.Value = "false";
                    rootNode.Attributes.Append(Okundu);

                    if (OcrMetin != null)
                    {
                        XmlAttribute Ocr = document.CreateAttribute("Ocr");
                        Ocr.Value = OcrMetin;
                        rootNode.Attributes.Append(Ocr);
                    }

                    XmlNode Gun = document.CreateElement("Gun");
                    Gun.InnerText = TamTarih.ToString("o");
                    rootNode.AppendChild(Gun);

                    XmlNode Aciklama = document.CreateElement("Aciklama");
                    Aciklama.InnerText = GünNotAçıklama;
                    rootNode.AppendChild(Aciklama);

                    if (ResimData != null && ResimUzantı != null)
                    {
                        XmlNode Resim = document.CreateElement("Resim");
                        rootNode.AppendChild(Resim);
                        XmlAttribute ResimExt = document.CreateAttribute("Ext");
                        ResimExt.Value = ResimUzantı;
                        Resim.Attributes.Append(ResimExt);
                        Resim.InnerText = Convert.ToBase64String(ResimData);
                    }

                    if (Dosyalar != null)
                    {
                        XmlNode xmlnodeDosyalar = document.CreateElement("Dosyalar");
                        rootNode.AppendChild(xmlnodeDosyalar);
                        WriteFileListtoXml(document, xmlnodeDosyalar);
                    }
                    document.DocumentElement.AppendChild(rootNode);
                    document.Save(MainViewModel.xmlpath);
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
                        Filter = "Resim Dosyaları (*.webp)|*.webp",
                        FileName = xmlElement.PreviousSibling?.InnerText + xmlElement.GetAttribute("Ext")
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        byte[] bytes = Convert.FromBase64String(xmlElement["Resim"].InnerText);
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
            }, parameter => parameter is byte[] && !OcrSürüyor && Environment.OSVersion.Version.Major > 5 && Directory.Exists(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\tessdata"));

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
                    foreach (XmlNode item in MainViewModel.xmlDataProvider.Document?.SelectNodes("/Veriler/Veri"))
                    {
                        if (item.Attributes["Id"].InnerText == Id.Value)
                        {
                            item.ParentNode.RemoveChild(item);
                        }
                    }
                    MainViewModel.xmlDataProvider.Document.Save(MainViewModel.xmlpath);
                    MainViewModel.xmlDataProvider.Refresh();
                    CollectionViewSource.GetDefaultView((Application.Current.MainWindow.DataContext as MainViewModel)?.AyGünler).Refresh();
                    VeriSayısı--;
                }
            }, parameter => true);

            CsvDosyasınaYaz = new RelayCommand<object>(parameter =>
            {
                XmlDocument doc = MainViewModel.xmlDataProvider.Document;
                string dosyaismi = Path.GetTempPath() + Guid.NewGuid() + ".csv";
                string seperator = new CultureInfo(CultureInfo.CurrentCulture.Name).TextInfo.ListSeparator;
                foreach (XmlNode item in doc.SelectNodes("//Veriler/Veri"))
                {
                    File.AppendAllText(dosyaismi, $"{item["Gun"]?.InnerText}{seperator}{item.Attributes["SaatBaslangic"]?.InnerText}{seperator}{item["Aciklama"]?.InnerText}\n", Encoding.UTF8);
                }
                Process.Start(dosyaismi);
            }, parameter => true);

            XmlVeriGüncelle = new RelayCommand<object>(parameter =>
            {
                if (parameter is XmlAttribute xmlattributeId)
                {
                    UpdateAttribute(xmlattributeId, "SaatBaslangic", SaatBaşlangıç);
                    UpdateAttribute(xmlattributeId, "Saat", EtkinlikSüresi.ToString());
                    UpdateAttribute(xmlattributeId, "AyTekrar", AyTekrar.ToString().ToLower());
                    UpdateAttribute(xmlattributeId, "Renk", VeriRenk.ToString());
                }
            }, parameter => DateTime.TryParseExact(SaatBaşlangıç, "H:m", new CultureInfo("tr-TR"), DateTimeStyles.None, out _));

            Resimİptal = new RelayCommand<object>(parameter => ResimData = null, parameter => ResimData?.Length > 0);

            Okunduİşaretle = new RelayCommand<object>(parameter =>
            {
                if (parameter is int id)
                {
                    UpdateAttribute(id, "Okundu", "true");
                }
            }, parameter => true);

            IcalEkle = new RelayCommand<object>(parameter =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = false, Filter = "Ical File (*.ics)|*.ics" };
                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        vCalendar vcalendar = new vCalendar(File.ReadAllText(openFileDialog.FileName));
                        foreach (vEvent data in vcalendar.vEvents)
                        {
                            foreach (System.Collections.Generic.KeyValuePair<string, ContentLine> item in data.ContentLines)
                            {
                                if (item.Key == "SUMMARY")
                                {
                                    GünNotAçıklama = item.Value.Value;
                                }

                                if (item.Key == "DTSTART")
                                {
                                    if (item.Value.Value.Length > 8)
                                    {
                                        SaatBaşlangıç = item.Value.Value.Substring(9, 2) + ":" + item.Value.Value.Substring(11, 2);
                                    }
                                    TamTarih = DateTime.ParseExact(item.Value.Value.Substring(0, 8), "yyyyMMdd", CultureInfo.CurrentCulture);
                                }
                            }
                            XmlVeriEkle.Execute(null);
                        }
                        TamTarih = DateTime.Today;
                        MessageBox.Show("Takvim Verileri Eklendi.", "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }, parameter => true);

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

        public ICommand Okunduİşaretle { get; }

        public ICommand DosyaAç { get; }

        public ICommand IcalEkle { get; }

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

        public bool KilitliMi
        {
            get => kilitliMi;

            set
            {
                if (kilitliMi != value)
                {
                    kilitliMi = value;
                    OnPropertyChanged(nameof(KilitliMi));
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

        public ICommand XmlVeriEkle { get; }

        public ICommand XmlVeriGüncelle { get; }

        public ICommand XmlVeriSil { get; }

        private void UpdateAttribute(XmlAttribute xmlAttribute, string attributevalue, string updatedattributevalue)
        {
            foreach (XmlNode item in MainViewModel.xmlDataProvider.Document?.SelectNodes("/Veriler/Veri"))
            {
                if (item.Attributes.GetNamedItem("Id").Value == xmlAttribute.Value)
                {
                    item.Attributes.GetNamedItem(attributevalue).Value = updatedattributevalue;
                }
            }
            MainViewModel.xmlDataProvider.Document.Save(MainViewModel.xmlpath);
            MainViewModel.xmlDataProvider.Refresh();
        }

        private void UpdateAttribute(int id, string attributevalue, string updatedattributevalue)
        {
            foreach (XmlNode item in MainViewModel.xmlDataProvider.Document?.SelectNodes("/Veriler/Veri"))
            {
                if (item.Attributes.GetNamedItem("Id").Value == id.ToString())
                {
                    item.Attributes.GetNamedItem(attributevalue).Value = updatedattributevalue;
                }
            }
            MainViewModel.xmlDataProvider.Document.Save(MainViewModel.xmlpath);
            MainViewModel.xmlDataProvider.Refresh();
        }

        private void WriteFileListtoXml(XmlDocument document, XmlNode xmlnodeDosyalar)
        {
            foreach (string dosya in Dosyalar)
            {
                XmlNode Dosya = document.CreateElement("Dosya");
                xmlnodeDosyalar.AppendChild(Dosya);
                XmlAttribute Yol = document.CreateAttribute("Yol");
                Yol.Value = dosya;
                XmlAttribute Ext = document.CreateAttribute("Ext");
                Ext.Value = Path.GetExtension(dosya);
                XmlAttribute Ad = document.CreateAttribute("Ad");
                Ad.Value = Path.GetFileNameWithoutExtension(dosya);
                Dosya.Attributes.Append(Yol);
                Dosya.Attributes.Append(Ext);
                Dosya.Attributes.Append(Ad);
            }
        }
    }
}
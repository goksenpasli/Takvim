using iCalNET.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public partial class Data : InpcBase, IDataErrorInfo
    {
        public Data()
        {
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
                    VeriRenk = typeof(Brushes).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(pi => (Brush)pi.GetValue(null, null)).Where(z => z != Brushes.Black && z != Brushes.White && z != Brushes.Transparent).OrderBy(_ => Guid.NewGuid()).Take(1).First();
                    Renk.Value = VeriRenk.ToString();
                    rootNode.Attributes.Append(Renk);

                    if (ÖnemliMi)
                    {
                        XmlAttribute Onemli = document.CreateAttribute("Onemli");
                        Onemli.Value = ÖnemliMi.ToString().ToLower();
                        rootNode.Attributes.Append(Onemli);
                    }

                    XmlAttribute TekrarGun = document.CreateAttribute("TekrarGun");
                    TekrarGun.Value = AyTekrarGun.ToString();
                    rootNode.Attributes.Append(TekrarGun);

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

                    if (ResimData != null && DosyaUzantı != null)
                    {
                        XmlNode Resim = document.CreateElement("Resim");
                        rootNode.AppendChild(Resim);
                        XmlAttribute ResimExt = document.CreateAttribute("Ext");
                        ResimExt.Value = DosyaUzantı;
                        Resim.Attributes.Append(ResimExt);
                        Resim.InnerText = Convert.ToBase64String(ResimData);
                    }

                    if (PdfData != null && DosyaUzantı != null)
                    {
                        XmlNode Pdf = document.CreateElement("Pdf");
                        rootNode.AppendChild(Pdf);
                        XmlAttribute PdfExt = document.CreateAttribute("Ext");
                        PdfExt.Value = DosyaUzantı;
                        Pdf.Attributes.Append(PdfExt);
                        Pdf.InnerText = Convert.ToBase64String(PdfData);
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
                OpenFileDialog openFileDialog = new() { Multiselect = false, Filter = "Resim Dosyaları (*.jpg;*.jpeg;*.tif;*.tiff;*.png)|*.jpg;*.jpeg;*.tif;*.tiff;*.png" };
                if (openFileDialog.ShowDialog() == true)
                {
                    ResimYolu = openFileDialog.FileName;
                    ResimData = ResimYolu.WebpEncode(WebpQuality);
                    DosyaUzantı = ".webp";
                    Boyut = ResimData.Length / 1024;
                }
            }, parameter => Environment.OSVersion.Version.Major > 5);

            PdfYükle = new RelayCommand<object>(parameter =>
            {
                OpenFileDialog openFileDialog = new() { Multiselect = false, Filter = "Pdf Dosyaları (*.pdf)|*.pdf" };
                if (openFileDialog.ShowDialog() == true)
                {
                    PdfData = File.ReadAllBytes(openFileDialog.FileName);
                    DosyaUzantı = ".pdf";
                }
            }, parameter => Environment.OSVersion.Version.Major > 5);

            ResimSakla = new RelayCommand<object>(parameter =>
            {
                if (parameter is XmlElement xmlElement)
                {
                    SaveFileDialog saveFileDialog = new()
                    {
                        Title = "SAKLA",
                        Filter = "Resim Dosyaları (*.webp)|*.webp",
                        FileName = xmlElement["Aciklama"]?.InnerText + xmlElement.GetAttribute("Ext")
                    };

                    if (saveFileDialog.ShowDialog() == true && xmlElement["Resim"]?.InnerText != null)
                    {
                        byte[] bytes = Convert.FromBase64String(xmlElement["Resim"].InnerText);
                        using FileStream imageFile = new(saveFileDialog.FileName, FileMode.Create);
                        imageFile.Write(bytes, 0, bytes.Length);
                        imageFile.Flush();
                    }
                }
            }, parameter => true);

            PencereKapat = new RelayCommand<object>(parameter => (parameter as Window)?.Close(), parameter => true);

            OcrUygula = new RelayCommand<object>(parameter =>
            {
                OcrTask = Task.Factory.StartNew(() =>
                {
                    OcrSürüyor = true;
                    OcrMetin = (parameter as byte[]).WebpDecode().ToTiffJpegByteArray(ExtensionMethods.Format.Jpg).OcrYap();
                    OcrSürüyor = false;
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            }, parameter => parameter is byte[] && !OcrSürüyor && Environment.OSVersion.Version.Major > 5 && Ocr.tesseractexsist);

            EkDosyaAç = new RelayCommand<object>(parameter =>
            {
                if (parameter is string yol)
                {
                    Process.Start(yol);
                }
            }, parameter => parameter is string yol && File.Exists(yol));

            DosyaGör = new RelayCommand<object>(parameter =>
            {
                if (parameter is XmlElement xmlElement)
                {
                    using Viewer viewer = new(xmlElement)
                    {
                        Owner = Application.Current.MainWindow
                    };
                    viewer.ShowDialog();
                }
            }, parameter => true);

            ArşivDosyasıEkle = new RelayCommand<object>(parameter =>
            {
                if (parameter is string arşivdosyayolu)
                {
                    if (File.Exists(arşivdosyayolu))
                    {
                        Dosyalar?.Add(arşivdosyayolu);
                    }
                    else
                    {
                        MessageBox.Show("Arşiv Dosyası Bulunamadı.", "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }, parameter => true);

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
                    VeriSayısı--;
                    MainViewModel.xmlDataProvider.Document.Save(MainViewModel.xmlpath);
                    MainViewModel.xmlDataProvider.Refresh();
                    CollectionViewSource.GetDefaultView((Application.Current.MainWindow.DataContext as MainViewModel)?.AyGünler).Refresh();
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
                    UpdateAttribute(xmlattributeId, "TekrarGun", AyTekrarGun.ToString());
                }
            }, parameter => EtkinlikSüresi is <= 24 and >= 0 && VeriRenk is not null && DateTime.TryParseExact(SaatBaşlangıç, "H:m", new CultureInfo("tr-TR"), DateTimeStyles.None, out _));

            XmlRenkGüncelle = new RelayCommand<object>(parameter =>
            {
                if (parameter is XmlElement xmlElement)
                {
                    UpdateAttribute(int.Parse(xmlElement.GetAttribute("Id")), "Renk", xmlElement.GetAttribute("Renk"));
                }
            }, parameter => true);

            Resimİptal = new RelayCommand<object>(parameter => ResimData = null, parameter => ResimData?.Length > 0);

            Pdfİptal = new RelayCommand<object>(parameter => PdfData = null, parameter => PdfData?.Length > 0);

            Okunduİşaretle = new RelayCommand<object>(parameter =>
            {
                if (parameter is int id)
                {
                    UpdateAttribute(id, "Okundu", "true");
                }
            }, parameter => true);

            IcalEkle = new RelayCommand<object>(parameter =>
            {
                OpenFileDialog openFileDialog = new() { Multiselect = false, Filter = "Ical File (*.ics)|*.ics" };
                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        VCalendar vcalendar = new(File.ReadAllText(openFileDialog.FileName));
                        foreach (VEvent data in vcalendar.vEvents)
                        {
                            foreach (KeyValuePair<string, ContentLine> item in data.ContentLines)
                            {
                                if (item.Key == "SUMMARY")
                                {
                                    GünNotAçıklama = item.Value.Value;
                                }

                                if (item.Key == "RRULE" && item.Value.Value.Contains("MONTHLY"))
                                {
                                    AyTekrar = true;
                                    string aytekrardata = item.Value.Value.Split(';')?[1];
                                    if (aytekrardata?.Contains("BYMONTHDAY") == true)
                                    {
                                        AyTekrarGun = Convert.ToDouble(aytekrardata?.Split('=')?[1]);
                                    }
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
                Window verigirişwindow = new()
                {
                    Title = TamTarih.ToString("dd MMMM yyyy dddd"),
                    Content = new DataEnterWindow(),
                    DataContext = parameter as Data,
                    Width = 504,
                    AllowsTransparency = true,
                    WindowStyle = WindowStyle.None,
                    Height = 454,
                    Background = Brushes.Transparent,
                    Owner = Application.Current.MainWindow,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                Dosyalar = new ObservableCollection<string>();
                verigirişwindow.MouseLeftButtonDown += (s, e) => verigirişwindow.DragMove();
                verigirişwindow.ShowDialog();
            }, parameter => true);
            PropertyChanged += Data_PropertyChanged;
        }

        public ICommand ArşivDosyasıEkle { get; }

        public ICommand CsvDosyasınaYaz { get; }

        public ICommand DosyaGör { get; }

        public ICommand EkDosyaAç { get; }

        public string Error => string.Empty;

        public ICommand IcalEkle { get; }

        public ICommand OcrUygula { get; }

        public ICommand Okunduİşaretle { get; }

        public ICommand Pdfİptal { get; }

        public ICommand PdfYükle { get; }

        public ICommand PencereKapat { get; }

        public ICommand Resimİptal { get; }

        public ICommand ResimSakla { get; }

        public ICommand ResimYükle { get; }

        public ICommand VeriEkleEkranı { get; }

        public ICommand XmlRenkGüncelle { get; }

        public ICommand XmlVeriEkle { get; }

        public ICommand XmlVeriGüncelle { get; }

        public ICommand XmlVeriSil { get; }

        public string this[string columnName] => columnName switch
        {
            "SaatBaşlangıç" when string.IsNullOrWhiteSpace(SaatBaşlangıç) || SaatBaşlangıç == "__:__" => "Başlangıç Saatini Boş Bırakmayın.",
            "GünNotAçıklama" when string.IsNullOrWhiteSpace(GünNotAçıklama) => "Açıklamayı Boş Bırakmayın.",
            "VeriRenk" when VeriRenk == null => "Renk Boş Bırakmayın.",
            "EtkinlikSüresi" when EtkinlikSüresi is > 24 or < 0 => "Etkinlik Süresini 0-24 Arasında Girin.",
            _ => null
        };

        private void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is "AyTekrar")
            {
                AyTekrarGun = AyTekrar ? DateTime.Now.Day : 0;
            }
            if (e.PropertyName is "WebpQuality" && ResimYolu is not null)
            {
                ResimData = ResimYolu.WebpEncode(WebpQuality);
                Boyut = ResimData.Length / 1024;
            }
        }

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
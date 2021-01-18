﻿using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace Takvim
{
    public class Data : InpcBase
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

        public Data()
        {
            Window verigirişwindow = null;

            XmlVeriEkle = new RelayCommand(parameter =>
            {
                if (!File.Exists(MainViewModel.xmlpath))
                {
                    using XmlWriter writer = XmlWriter.Create(MainViewModel.xmlpath);
                    writer.WriteStartElement("Veriler");
                    writer.WriteEndElement();
                    writer.Flush();
                }

                var xDocument = XDocument.Load(MainViewModel.xmlpath);
                var parentElement = new XElement("Veri");
                parentElement.Add(new XAttribute("Id", new Random().Next(1, int.MaxValue)));
                parentElement.Add(new XAttribute("Onemli", ÖnemliMi));
                parentElement.Add(new XAttribute("Saat", EtkinlikSüresi));
                var xmlcontent = new object[3];
                xmlcontent[0] = new XElement("Gun", TamTarih);
                xmlcontent[1] = new XElement("Aciklama", GünNotAçıklama);
                if (ResimData != null && ResimUzantı != null)
                {
                    var xElement = new XElement("Resim", Convert.ToBase64String(ResimData));
                    xElement.Add(new XAttribute("Ext", ResimUzantı));
                    xmlcontent[2] = xElement;
                }
                if (VeriRenk != null)
                {
                    parentElement.Add(new XAttribute("Renk", VeriRenk));
                }
                parentElement.Add(xmlcontent);
                xDocument.Element("Veriler")?.Add(parentElement);
                xDocument.Save(MainViewModel.xmlpath);
                verigirişwindow.Close();
                VeriSayısı++;
                MainViewModel.xmlDataProvider.Refresh();
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

            VeriEkleEkranı = new RelayCommand(parameter =>
            {
                verigirişwindow = new Window
                {
                    Title = TamTarih.ToShortDateString(),
                    Content = new DataEnterWindow(),
                    DataContext = this,
                    Width = 300,
                    WindowStyle = WindowStyle.ToolWindow,
                    Height = 200,
                    Owner = Application.Current.MainWindow,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                verigirişwindow.ShowDialog();
            }, parameter => true);
        }

        public ICommand XmlVeriEkle { get; }

        public ICommand ResimYükle { get; }

        public ICommand ResimSakla { get; }

        public ICommand VeriEkleEkranı { get; }
    }
}
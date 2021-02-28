using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{
    public class Prayer : InpcBase
    {
        private string akşam;

        private int cityID;

        private string ikindi;

        private ObservableCollection<Prayer> list;

        private string öğle;

        private string sabah;

        private string şehir;

        private string yatsı;

        private DateTime yılınGünü;

        public Prayer()
        {
        }

        public Prayer(XmlDataProvider xmlDataProvider)
        {
            XmlDataProvider = xmlDataProvider;
            PropertyChanged += Prayer_PropertyChanged;
        }

        public string Akşam
        {
            get => akşam;
            set
            {
                if (akşam != value)
                {
                    akşam = value;
                    OnPropertyChanged(nameof(Akşam));
                }
            }
        }

        public int CityID
        {
            get => cityID;

            set
            {
                if (cityID != value)
                {
                    cityID = value;
                    OnPropertyChanged(nameof(CityID));
                }
            }
        }

        public IEnumerable<int> IllerId { get; set; } = Enumerable.Range(16702, 81);

        public string İkindi
        {
            get => ikindi;
            set
            {
                if (ikindi != value)
                {
                    ikindi = value;
                    OnPropertyChanged(nameof(İkindi));
                }
            }
        }

        public ObservableCollection<Prayer> List
        {
            get => list;

            set
            {
                if (list != value)
                {
                    list = value;
                    OnPropertyChanged(nameof(List));
                }
            }
        }

        public string Öğle
        {
            get => öğle;

            set
            {
                if (öğle != value)
                {
                    öğle = value;
                    OnPropertyChanged(nameof(Öğle));
                }
            }
        }

        public string Sabah
        {
            get => sabah;

            set
            {
                if (sabah != value)
                {
                    sabah = value;
                    OnPropertyChanged(nameof(Sabah));
                }
            }
        }

        public string Şehir
        {
            get => şehir;

            set
            {
                if (şehir != value)
                {
                    şehir = value;
                    OnPropertyChanged(nameof(Şehir));
                }
            }
        }

        public DateTime Tarih
        {
            get => yılınGünü;

            set
            {
                if (yılınGünü != value)
                {
                    yılınGünü = value;
                    OnPropertyChanged(nameof(Tarih));
                }
            }
        }

        public XmlDataProvider XmlDataProvider { get; }

        public string Yatsı
        {
            get => yatsı;
            set
            {
                if (yatsı != value)
                {
                    yatsı = value;
                    OnPropertyChanged(nameof(Yatsı));
                }
            }
        }

        private void Prayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CityID")
            {
                try
                {
                    XmlDataProvider.Source = new Uri($"https://www.namazvakti.com/XML.php?cityID={CityID}");
                    List = new ObservableCollection<Prayer>();
                    Şehir = XmlDataProvider.Document?.SelectNodes("/cityinfo").Cast<XmlNode>().FirstOrDefault()?.Attributes.GetNamedItem("cityNameTR").Value;
                    string yıl = DateTime.Now.Year.ToString();
                    foreach (XmlNode item in XmlDataProvider.Document?.SelectNodes("/cityinfo/prayertimes"))
                    {
                        Prayer data = new Prayer()
                        {
                            Tarih = DateTime.Parse(item.Attributes.GetNamedItem("day").Value + "/" + item.Attributes.GetNamedItem("month").Value + "/" + yıl),
                            Sabah = item.InnerText.Split('\t')[2],
                            Öğle = item.InnerText.Split('\t')[5],
                            İkindi = item.InnerText.Split('\t')[6],
                            Akşam = item.InnerText.Split('\t')[9],
                            Yatsı = item.InnerText.Split('\t')[11]
                        };
                        List.Add(data);
                    }
                    XmlDataProvider.Source = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

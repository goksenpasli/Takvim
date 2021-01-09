using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;

namespace Takvim
{
    public class MainViewModel : InpcBase
    {
        public static readonly string xmlpath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + @"\Data.xml";

        public static XmlDataProvider xmlDataProvider;

        private readonly XmlDocument xmldoc;

        private ObservableCollection<Data> günler;

        public ObservableCollection<Data> Günler
        {
            get => günler;

            set
            {
                if (günler != value)
                {
                    günler = value;
                    OnPropertyChanged(nameof(Günler));
                }
            }
        }

        private int seçiliYıl = DateTime.Now.Year;

        private int bugünIndex = DateTime.Today.DayOfYear - 1;

        public int SeçiliYıl
        {
            get => seçiliYıl;

            set
            {
                if (seçiliYıl != value)
                {
                    seçiliYıl = value;
                    OnPropertyChanged(nameof(SeçiliYıl));
                }
            }
        }

        public int BugünIndex
        {
            get => bugünIndex;

            set
            {
                if (bugünIndex != value)
                {
                    bugünIndex = value;
                    OnPropertyChanged(nameof(BugünIndex));
                }
            }
        }

        public MainViewModel()
        {
            xmlDataProvider = (XmlDataProvider)Application.Current.MainWindow.TryFindResource("XmlData");
            xmlDataProvider.Source = new Uri(xmlpath);

            xmldoc = new XmlDocument();
            if (File.Exists(xmlpath))
            {
                xmldoc.Load(xmlpath);
            }

            TakvimVerileriniOluştur(SeçiliYıl);

            Geri = new RelayCommand(parameter =>
            {
                SeçiliYıl--;
                TakvimVerileriniOluştur(SeçiliYıl);
            }, parameter => SeçiliYıl > 1);

            İleri = new RelayCommand(parameter =>
            {
                SeçiliYıl++;
                TakvimVerileriniOluştur(SeçiliYıl);
            }, parameter => SeçiliYıl < 9999);
        }

        public ICommand Geri { get; }

        public ICommand İleri { get; }

        private ObservableCollection<Data> TakvimVerileriniOluştur(int SeçiliYıl)
        {
            Günler = new ObservableCollection<Data>();

            for (int i = 1; i <= 12; i++)
            {
                for (int j = 1; j <= 31; j++)
                {
                    string tarih = $"{j}.{i}.{SeçiliYıl}";
                    if (DateTime.TryParse(tarih, out _))
                    {
                        var data = new Data();
                        data.GünAdı = DateTime.Parse(tarih).ToString("ddd");
                        data.Gün = DateTime.Parse(tarih).Day;
                        data.Ay = DateTime.Parse(tarih).ToString("MMMM");
                        data.Offset = (int)DateTime.Parse(tarih).DayOfWeek;
                        data.TamTarih = DateTime.Parse(tarih);

                        foreach (XmlNode xn in xmldoc.SelectNodes("/Veriler/Veri"))
                        {
                            if (DateTime.Parse(xn["Gun"].InnerText) == data.TamTarih)
                            {
                                data.GünNotAçıklama = xn["Aciklama"].InnerText;
                            }

                            if (DateTime.Parse(xn["Gun"].InnerText) == data.TamTarih && xn["Resim"]?.InnerText != null)
                            {
                                data.ResimData = Convert.FromBase64String(xn["Resim"].InnerText);
                            }
                        }
                        Günler.Add(data);
                    }
                }
            }
            return Günler;
        }
    }
}
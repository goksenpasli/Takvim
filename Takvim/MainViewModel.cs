using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace Takvim
{
    public class MainViewModel : InpcBase, IDataErrorInfo
    {
        public static readonly string xmlpath = AppDomain.CurrentDomain.BaseDirectory + @"\Data.xml";

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

        private int sütünSayısı = 4;

        private int satırSayısı = 3;

        private Brush seçiliRenkCmt = ConvertToBrush(Properties.Settings.Default.CmtRenk);

        private Brush seçiliRenkPaz = ConvertToBrush(Properties.Settings.Default.PazRenk);

        private Brush resmiTatilRenk = ConvertToBrush(Properties.Settings.Default.ResmiTatil);

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

        public int SütünSayısı
        {
            get => sütünSayısı;

            set
            {
                if (sütünSayısı != value)
                {
                    sütünSayısı = value;
                    OnPropertyChanged(nameof(SütünSayısı));
                }
            }
        }

        public int SatırSayısı
        {
            get => satırSayısı;
            set
            {
                if (satırSayısı != value)
                {
                    satırSayısı = value;
                    OnPropertyChanged(nameof(SatırSayısı));
                }
            }
        }

        public Brush SeçiliRenkCmt
        {
            get => seçiliRenkCmt;

            set
            {
                if (seçiliRenkCmt != value)
                {
                    seçiliRenkCmt = value;
                    OnPropertyChanged(nameof(SeçiliRenkCmt));
                }
            }
        }

        public Brush SeçiliRenkPaz
        {
            get => seçiliRenkPaz;

            set
            {
                if (seçiliRenkPaz != value)
                {
                    seçiliRenkPaz = value;
                    OnPropertyChanged(nameof(SeçiliRenkPaz));
                }
            }
        }

        public Brush ResmiTatilRenk
        {
            get => resmiTatilRenk;

            set
            {
                if (resmiTatilRenk != value)
                {
                    resmiTatilRenk = value;
                    OnPropertyChanged(nameof(ResmiTatilRenk));
                }
            }
        }

        public string Error => string.Empty;

        public string this[string columnName] =>
            columnName switch
            {
                "SeçiliYıl" when SeçiliYıl <= 0 || SeçiliYıl > 9999 => "Seçili Yıl 1-9999 Aralığındadır.",
                _ => null
            };

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

            Geri = new RelayCommand(parameter => SeçiliYıl--, parameter => SeçiliYıl > 1);

            İleri = new RelayCommand(parameter => SeçiliYıl++, parameter => SeçiliYıl < 9999);

            SatırSütünSıfırla = new RelayCommand(parameter =>
            {
                SatırSayısı = 3;
                SütünSayısı = 4;
            }, parameter => SatırSayısı != 3 || SütünSayısı != 4);

            YılaGit = new RelayCommand(parameter =>
            {
                if (parameter is XmlElement xmlElement)
                {
                    SeçiliYıl = DateTime.Parse(xmlElement.InnerText).Year;
                }
            }, parameter => true);

            PropertyChanged += MainViewModel_PropertyChanged;
        }

        private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SütünSayısı")
            {
                if (12 % SütünSayısı == 0)
                {
                    SatırSayısı = 12 / SütünSayısı;
                }
                if (SatırSayısı * SütünSayısı < 12)
                {
                    SatırSayısı++;
                }
            }

            if (e.PropertyName == "SatırSayısı")
            {
                if (12 % SatırSayısı == 0)
                {
                    SütünSayısı = 12 / SatırSayısı;
                }
                if (SatırSayısı * SütünSayısı < 12)
                {
                    SütünSayısı++;
                }
            }

            if (e.PropertyName == "SeçiliYıl" && SeçiliYıl > 0 && SeçiliYıl < 10000)
            {
                TakvimVerileriniOluştur(SeçiliYıl);
            }

            if (e.PropertyName == "SeçiliRenkPaz" || e.PropertyName == "SeçiliRenkCmt" || e.PropertyName == "ResmiTatilRenk")
            {
                Properties.Settings.Default.PazRenk = ConvertToColor(SeçiliRenkPaz);
                Properties.Settings.Default.CmtRenk = ConvertToColor(SeçiliRenkCmt);
                Properties.Settings.Default.ResmiTatil = ConvertToColor(ResmiTatilRenk);
                Properties.Settings.Default.Save();
            }
        }

        public ICommand Geri { get; }

        public ICommand İleri { get; }

        public ICommand SatırSütünSıfırla { get; }

        public ICommand YılaGit { get; }

        private ObservableCollection<Data> TakvimVerileriniOluştur(int SeçiliYıl)
        {
            Günler = new ObservableCollection<Data>();
            for (int i = 1; i <= 12; i++)
            {
                for (int j = 1; j <= 31; j++)
                {
                    string tarih = $"{j}.{i}.{SeçiliYıl:0000}";
                    if (DateTime.TryParse(tarih, out DateTime date))
                    {
                        var data = new Data
                        {
                            GünAdı = date.ToString("ddd"),
                            Gün = date.Day,
                            Ay = date.ToString("MMMM"),
                            Offset = (int)date.DayOfWeek,
                            TamTarih = date
                        };

                        foreach (var xn in from XmlNode xn in xmldoc.SelectNodes("/Veriler/Veri") where DateTime.Parse(xn["Gun"].InnerText) == data.TamTarih select xn)
                        {
                            data.GünNotAçıklama = xn["Aciklama"].InnerText;
                            if (string.Equals(xn.Attributes.GetNamedItem("Onemli").Value, "true", StringComparison.CurrentCultureIgnoreCase))
                            {
                                data.ÖnemliMi = true;
                            }

                            if (xn["Resim"]?.InnerText != null)
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

        private static Brush ConvertToBrush(System.Drawing.Color color) => new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));

        private static System.Drawing.Color ConvertToColor(Brush color)
        {
            var t = (SolidColorBrush)color;
            return System.Drawing.Color.FromArgb(t.Color.A, t.Color.R, t.Color.G, t.Color.B);
        }
    }
}
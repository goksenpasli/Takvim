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

        public ObservableCollection<Data> AyGünler
        {
            get => ayGünler;

            set
            {
                if (ayGünler != value)
                {
                    ayGünler = value;
                    OnPropertyChanged(nameof(AyGünler));
                }
            }
        }

        private short seçiliYıl = (short)DateTime.Now.Year;

        private short bugünIndex = (short)(DateTime.Today.DayOfYear - 1);

        private short sütünSayısı = Properties.Settings.Default.Sütün;

        private short satırSayısı = Properties.Settings.Default.Satır;

        private Brush seçiliRenkCmt = Properties.Settings.Default.CmtRenk.ConvertToBrush();

        private Brush seçiliRenkPaz = Properties.Settings.Default.PazRenk.ConvertToBrush();

        private Brush resmiTatilRenk = Properties.Settings.Default.ResmiTatil.ConvertToBrush();

        private Brush gövdeRenk = Properties.Settings.Default.GövdeRenk.ConvertToBrush();

        private ObservableCollection<Data> ayGünler;

        private short seçiliAy= (short)DateTime.Now.Month;

        public short SeçiliYıl
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

        public short SeçiliAy
        {
            get => seçiliAy;

            set
            {
                if (seçiliAy != value)
                {
                    seçiliAy = value;
                    OnPropertyChanged(nameof(SeçiliAy));
                }
            }
        }

        public short BugünIndex
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

        public short SütünSayısı
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

        public short SatırSayısı
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

        public Brush GövdeRenk
        {
            get => gövdeRenk;

            set
            {
                if (gövdeRenk != value)
                {
                    gövdeRenk = value;
                    OnPropertyChanged(nameof(GövdeRenk));
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
            AyTakvimVerileriniOluştur(SeçiliAy);

            YılGeri = new RelayCommand(parameter => SeçiliYıl--, parameter => SeçiliYıl > 1);

            AyGeri = new RelayCommand(parameter => SeçiliAy--, parameter => SeçiliAy > 1);

            Yılİleri = new RelayCommand(parameter => SeçiliYıl++, parameter => SeçiliYıl < 9999);

            Ayİleri = new RelayCommand(parameter => SeçiliAy++, parameter => SeçiliAy < 12);

            SatırSütünSıfırla = new RelayCommand(parameter =>
            {
                SatırSayısı = 3;
                SütünSayısı = 4;
                Properties.Settings.Default.Save();
            }, parameter => SatırSayısı != 3 || SütünSayısı != 4);

            YılaGit = new RelayCommand(parameter =>
            {
                if (parameter is XmlElement xmlElement)
                {
                    SeçiliYıl = (short)DateTime.Parse(xmlElement.InnerText).Year;
                }
            }, parameter => parameter is XmlElement xmlElement && DateTime.Parse(xmlElement.InnerText).Year != SeçiliYıl);

            AyarSıfırla = new RelayCommand(parameter =>
            {
                Properties.Settings.Default.Reset();
                MessageBox.Show("Renk Ayarları Varsayılana Çevrildi. Yeniden Başlatın.", "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }, parameter => true);

            PropertyChanged += MainViewModel_PropertyChanged;
        }

        private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SütünSayısı")
            {
                if (12 % SütünSayısı == 0)
                {
                    SatırSayısı = (short)(12 / SütünSayısı);
                }
                if (SatırSayısı * SütünSayısı < 12)
                {
                    SatırSayısı++;
                }
                SaveColumnRowSettings();
            }

            if (e.PropertyName == "SatırSayısı")
            {
                if (12 % SatırSayısı == 0)
                {
                    SütünSayısı = (short)(12 / SatırSayısı);
                }
                if (SatırSayısı * SütünSayısı < 12)
                {
                    SütünSayısı++;
                }
                SaveColumnRowSettings();
            }

            if (e.PropertyName == "SeçiliYıl" && SeçiliYıl > 0 && SeçiliYıl < 10000)
            {
                TakvimVerileriniOluştur(SeçiliYıl);
            }     
            
            if (e.PropertyName == "SeçiliAy" && SeçiliAy > 0 && SeçiliAy < 13)
            {
                AyTakvimVerileriniOluştur(SeçiliAy);
            }

            if (e.PropertyName == "SeçiliRenkPaz" || e.PropertyName == "GövdeRenk" || e.PropertyName == "SeçiliRenkCmt" || e.PropertyName == "ResmiTatilRenk")
            {
                Properties.Settings.Default.PazRenk = SeçiliRenkPaz.ConvertToColor();
                Properties.Settings.Default.CmtRenk = SeçiliRenkCmt.ConvertToColor();
                Properties.Settings.Default.ResmiTatil = ResmiTatilRenk.ConvertToColor();
                Properties.Settings.Default.GövdeRenk = GövdeRenk.ConvertToColor();
                Properties.Settings.Default.Save();
            }

            void SaveColumnRowSettings()
            {
                Properties.Settings.Default.Satır = SatırSayısı;
                Properties.Settings.Default.Sütün = SütünSayısı;
                Properties.Settings.Default.Save();
            }
        }

        public ICommand AyGeri { get; }

        public ICommand YılGeri { get; }

        public ICommand Ayİleri { get; }

        public ICommand Yılİleri { get; }

        public ICommand SatırSütünSıfırla { get; }

        public ICommand YılaGit { get; }

        public ICommand AyarSıfırla { get; }

        private ObservableCollection<Data> TakvimVerileriniOluştur(short SeçiliYıl)
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
                            Gün = (short)date.Day,
                            Ay = date.ToString("MMMM"),
                            Offset = (short)date.DayOfWeek,
                            TamTarih = date
                        };

                        foreach (var xn in from XmlNode xn in xmldoc.SelectNodes("/Veriler/Veri") where DateTime.Parse(xn["Gun"].InnerText) == data.TamTarih select xn)
                        {
                            data.Id = Convert.ToInt32(xn.Attributes.GetNamedItem("Id").Value);
                        }

                        Günler.Add(data);
                    }
                }
            }
            return Günler;
        }

        private ObservableCollection<Data> AyTakvimVerileriniOluştur(short SeçiliAy)
        {
            AyGünler= new ObservableCollection<Data>(Günler.Where(z => z.TamTarih.Month == SeçiliAy));
            return AyGünler;
        }
    }
}
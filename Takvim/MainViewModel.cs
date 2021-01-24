using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

        public static System.Windows.Forms.NotifyIcon AppNotifyIcon;

        public static WindowState AppWindowState = WindowState.Maximized;

        public static XmlDataProvider xmlDataProvider;

        private readonly CollectionViewSource Cvs = (CollectionViewSource)Application.Current.MainWindow.TryFindResource("Cvs");

        private readonly XmlDocument xmldoc;

        private string aramaMetin;

        private ObservableCollection<Data> ayGünler;

        private Brush bayramTatilRenk = Properties.Settings.Default.BayramRenk.ConvertToBrush();

        private short bugünIndex = (short)(DateTime.Today.DayOfYear - 1);

        private Brush gövdeRenk = Properties.Settings.Default.GövdeRenk.ConvertToBrush();

        private ObservableCollection<Data> günler;

        private Brush resmiTatilRenk = Properties.Settings.Default.ResmiTatil.ConvertToBrush();

        private short satırSayısı = Properties.Settings.Default.Satır;

        private short seçiliAy = (short)DateTime.Now.Month;

        private Brush seçiliRenkCmt = Properties.Settings.Default.CmtRenk.ConvertToBrush();

        private Brush seçiliRenkPaz = Properties.Settings.Default.PazRenk.ConvertToBrush();

        private short seçiliYıl = (short)DateTime.Now.Year;

        private short sütünSayısı = Properties.Settings.Default.Sütün;

        private bool başlangıçtaÇalışacak = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true).GetValue("Takvim") != null;

        public MainViewModel()
        {
            AppNotifyIcon = new System.Windows.Forms.NotifyIcon
            {
                BalloonTipText = "Uygulama Sistem Tepsisine Gönderildi.",
                BalloonTipTitle = "TAKVİM",
                Text = "Takvim",
                Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/Takvim;component/icon.ico")).Stream)
            };
            AppNotifyIcon.Click += (s, e) =>
             {
                 Application.Current.MainWindow.Show();
                 Application.Current.MainWindow.WindowState = AppWindowState;
             };

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

            VeriAra = new RelayCommand(parameter => Cvs.Filter += (s, e) => e.Accepted = (e.Item as XmlNode)?["Aciklama"].InnerText.Contains(AramaMetin) == true, parameter => !string.IsNullOrWhiteSpace(AramaMetin));

            PropertyChanged += MainViewModel_PropertyChanged;
        }

        public string AramaMetin
        {
            get => aramaMetin;

            set
            {
                if (aramaMetin != value)
                {
                    aramaMetin = value;
                    OnPropertyChanged(nameof(AramaMetin));
                }
            }
        }

        public ICommand AyarSıfırla { get; }

        public ICommand AyGeri { get; }

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

        public ICommand Ayİleri { get; }

        public Brush BayramTatilRenk
        {
            get => bayramTatilRenk;

            set
            {
                if (bayramTatilRenk != value)
                {
                    bayramTatilRenk = value;
                    OnPropertyChanged(nameof(BayramTatilRenk));
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

        public string Error => string.Empty;

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

        public ICommand SatırSütünSıfırla { get; }

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

        public bool BaşlangıçtaÇalışacak
        {
            get => başlangıçtaÇalışacak;

            set
            {
                if (başlangıçtaÇalışacak != value)
                {
                    başlangıçtaÇalışacak = value;
                    OnPropertyChanged(nameof(BaşlangıçtaÇalışacak));
                }
            }
        }

        public ICommand VeriAra { get; }

        public ICommand YılaGit { get; }

        public ICommand YılGeri { get; }

        public ICommand Yılİleri { get; }

        public string this[string columnName] => columnName switch
        {
            "SeçiliYıl" when SeçiliYıl <= 0 || SeçiliYıl > 9999 => "Seçili Yıl 1-9999 Aralığındadır.",
            _ => null
        };

        private ObservableCollection<Data> AyTakvimVerileriniOluştur(short SeçiliAy)
        {
            AyGünler = new ObservableCollection<Data>(Günler.Where(z => z.TamTarih.Month == SeçiliAy));
            return AyGünler;
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

            if (e.PropertyName == "SeçiliRenkPaz" || e.PropertyName == "GövdeRenk" || e.PropertyName == "SeçiliRenkCmt" || e.PropertyName == "ResmiTatilRenk" || e.PropertyName == "BayramTatilRenk")
            {
                Properties.Settings.Default.PazRenk = SeçiliRenkPaz.ConvertToColor();
                Properties.Settings.Default.CmtRenk = SeçiliRenkCmt.ConvertToColor();
                Properties.Settings.Default.ResmiTatil = ResmiTatilRenk.ConvertToColor();
                Properties.Settings.Default.GövdeRenk = GövdeRenk.ConvertToColor();
                Properties.Settings.Default.BayramRenk = BayramTatilRenk.ConvertToColor();
                Properties.Settings.Default.Save();
            }

            if (e.PropertyName == "AramaMetin" && string.IsNullOrWhiteSpace(AramaMetin))
            {
                Cvs.View.Filter = null;
            }

            if (e.PropertyName == "BaşlangıçtaÇalışacak")
            {
                BaşlangıçtaÇalıştır(BaşlangıçtaÇalışacak);
            }

            void SaveColumnRowSettings()
            {
                Properties.Settings.Default.Satır = SatırSayısı;
                Properties.Settings.Default.Sütün = SütünSayısı;
                Properties.Settings.Default.Save();
            }
        }
        private ObservableCollection<Data> TakvimVerileriniOluştur(short SeçiliYıl)
        {
            XmlNodeList xmlNodeList = xmldoc.SelectNodes("/Veriler/Veri");
            Günler = new ObservableCollection<Data>();
            for (int i = 1; i <= 12; i++)
            {
                for (int j = 1; j <= 31; j++)
                {
                    string tarih = $"{j}.{i}.{SeçiliYıl:0000}";
                    if (DateTime.TryParse(tarih, out DateTime date))
                    {
                        Data data = new Data
                        {
                            GünAdı = date.ToString("ddd"),
                            Gün = (short)date.Day,
                            Ay = date.ToString("MMMM"),
                            Offset = (short)date.DayOfWeek,
                            TamTarih = date
                        };

                        foreach (XmlNode xn in from XmlNode xn in xmlNodeList where DateTime.Parse(xn["Gun"].InnerText) == data.TamTarih select xn)
                        {
                            data.Id = Convert.ToInt32(xn.Attributes.GetNamedItem("Id").Value);
                        }

                        Günler.Add(data);
                    }
                }
            }
            return Günler;
        }

        private void BaşlangıçtaÇalıştır(bool isChecked)
        {
            try
            {
                using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (isChecked)
                {
                    registryKey.SetValue("Takvim", $@"""{Process.GetCurrentProcess().MainModule.FileName}"" /MINIMIZE");
                }
                else
                {
                    registryKey.DeleteValue("Takvim");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Takvim", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}
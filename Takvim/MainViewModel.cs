using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace Takvim
{
    public class MainViewModel : InpcBase, IDataErrorInfo
    {
        public static readonly string xmldatasavefolder = Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);

        public static readonly string xmlpath = Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath) + @"\Data.xml";

        public static System.Windows.Forms.NotifyIcon AppNotifyIcon;

        public static WindowState AppWindowState = WindowState.Maximized;

        public static Window duyurularwindow;

        public static XmlDataProvider xmlDataProvider;

        private readonly CollectionViewSource Cvs = (CollectionViewSource)Application.Current.MainWindow.TryFindResource("Cvs");

        private DateTime? animasyonTarih;

        private string aramaMetin;

        private ObservableCollection<Data> ayGünler;

        private bool başlangıçtaÇalışacak = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true).GetValue("Takvim") != null;

        private Brush bayramTatilRenk = Properties.Settings.Default.BayramRenk.ConvertToBrush();

        private string etkinlik;

        private Data şuankigünData=new Data();

        private Brush gövdeRenk = Properties.Settings.Default.GövdeRenk.ConvertToBrush();

        private ObservableCollection<Data> günler;

        private Brush resmiTatilRenk = Properties.Settings.Default.ResmiTatil.ConvertToBrush();

        private short satırSayısı = Properties.Settings.Default.Satır;

        private short seçiliAy = (short)DateTime.Now.Month;

        private Brush seçiliRenkCmt = Properties.Settings.Default.CmtRenk.ConvertToBrush();

        private Brush seçiliRenkPaz = Properties.Settings.Default.PazRenk.ConvertToBrush();

        private short seçiliYıl = (short)DateTime.Now.Year;

        private short sütünSayısı = Properties.Settings.Default.Sütün;

        private ObservableCollection<Data> yaklaşanEtkinlikler;

        private bool tümListe;

        public MainViewModel()
        {
            System.Windows.Forms.ContextMenu contextmenu = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem menuitem = new System.Windows.Forms.MenuItem
            {
                Index = 0,
                Text = "VERİ EKLE"
            };
            contextmenu.MenuItems.Add(menuitem);
            AppNotifyIcon = new System.Windows.Forms.NotifyIcon
            {
                BalloonTipText = "Uygulama Sistem Tepsisine Gönderildi.",
                BalloonTipTitle = "TAKVİM",
                Text = "Takvim",
                Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/Takvim;component/icon.ico")).Stream),
                ContextMenu = contextmenu
            };

            menuitem.Click += (s, e) =>
            {
                Data data = new Data
                {
                    TamTarih = DateTime.Today
                };
                data.VeriEkleEkranı.Execute(null);
            };

            AppNotifyIcon.Click += (s, e) =>
             {
                 Application.Current.MainWindow.Show();
                 Application.Current.MainWindow.WindowState = AppWindowState;
             };

            WriteXmlRootData(xmlpath);
            xmlDataProvider = (XmlDataProvider)Application.Current.TryFindResource("XmlData");
            xmlDataProvider.Source = new Uri(xmlpath);

            TakvimVerileriniOluştur(SeçiliYıl);

            AyTakvimVerileriniOluştur(SeçiliAy);

            YılGeri = new RelayCommand<object>(parameter => SeçiliYıl--, parameter => SeçiliYıl > 1);

            AyGeri = new RelayCommand<object>(parameter => SeçiliAy--, parameter => SeçiliAy > 1);

            Yılİleri = new RelayCommand<object>(parameter => SeçiliYıl++, parameter => SeçiliYıl < 9999);

            Ayİleri = new RelayCommand<object>(parameter => SeçiliAy++, parameter => SeçiliAy < 12);

            SatırSütünSıfırla = new RelayCommand<object>(parameter =>
            {
                SatırSayısı = 3;
                SütünSayısı = 4;
                Properties.Settings.Default.Save();
            }, parameter => SatırSayısı != 3 || SütünSayısı != 4);

            YılaGit = new RelayCommand<object>(parameter =>
            {
                if (parameter is XmlElement xmlElement)
                {
                    SeçiliYıl = (short)DateTime.Parse(xmlElement.InnerText).Year;
                    AnimasyonTarih = null;
                    AnimasyonTarih = DateTime.Parse(xmlElement.InnerText);
                }
            }, parameter => true);

            ResimGör = new RelayCommand<object>(parameter =>
            {
                if (parameter is XmlElement xmlElement)
                {
                    using Viewer viewer = new Viewer(xmlElement)
                    {
                        Owner = App.Current.MainWindow
                    };
                    viewer.ShowDialog();
                }
            }, parameter => true);

            AyarSıfırla = new RelayCommand<object>(parameter =>
            {
                Properties.Settings.Default.Reset();
                MessageBox.Show("Ayarlar Varsayılana Çevrildi. Yeniden Başlatın.", "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }, parameter => true);

            EskiVerileriSil = new RelayCommand<object>(parameter =>
            {
                if (MessageBox.Show($"{SeçiliYıl} yılına ait tüm kayıtları silmek istiyor musun? Dikkat bu işlem geri alınamaz.", "TAKVİM", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    XDocument doc = XDocument.Load(MainViewModel.xmlpath);
                    doc.Root.Elements("Veri").Where(z => DateTime.Parse(z.Element("Gun").Value).Year == SeçiliYıl).Remove();
                    doc.Save(MainViewModel.xmlpath);
                    MainViewModel.xmlDataProvider.Refresh();
                }
            }, parameter => SeçiliYıl < DateTime.Now.Year && File.Exists(xmlpath));

            DuyurularPopupEkranıAç = new RelayCommand<object>(parameter =>
            {
                duyurularwindow = new Window
                {
                    Content = new FloatingWindowControl(),
                    DataContext = this,
                    Width = 225,
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true,
                    Height = 150,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false,
                    Topmost = true,
                    Background = Brushes.Transparent,
                    Top = SystemParameters.PrimaryScreenHeight - 200,
                    Left = SystemParameters.PrimaryScreenWidth - 250,
                };
                YaklaşanEtkinlikleriAl();
                if (YaklaşanEtkinlikler.Any())
                {
                    duyurularwindow.Show();
                }
            }, parameter => true);

            VeriAra = new RelayCommand<object>(parameter =>
            {
                TümListe = true;
                Cvs.Filter += (s, e) => e.Accepted = (e.Item as XmlNode)?["Aciklama"]?.InnerText.Contains(AramaMetin) == true || (e.Item as XmlNode)?.Attributes.GetNamedItem("Ocr")?.InnerText.Contains(AramaMetin, StringComparison.OrdinalIgnoreCase) == true;
            }, parameter => !string.IsNullOrWhiteSpace(AramaMetin));

            ŞuAnkiGünData.TamTarih = DateTime.Today;

            PropertyChanged += MainViewModel_PropertyChanged;
            Properties.Settings.Default.PropertyChanged += Properties_PropertyChanged;
        }

        public DateTime? AnimasyonTarih
        {
            get => animasyonTarih;

            set
            {
                if (animasyonTarih != value)
                {
                    animasyonTarih = value;
                    OnPropertyChanged(nameof(AnimasyonTarih));
                }
            }
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

        public bool TümListe
        {
            get => tümListe;

            set
            {
                if (tümListe != value)
                {
                    tümListe = value;
                    OnPropertyChanged(nameof(TümListe));
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

        public ICommand DuyurularPopupEkranıAç { get; }

        public string Error => string.Empty;

        public ICommand EskiVerileriSil { get; }

        public string Etkinlik
        {
            get => etkinlik;

            set
            {
                if (etkinlik != value)
                {
                    etkinlik = value;
                    OnPropertyChanged(nameof(Etkinlik));
                }
            }
        }

        public Data ŞuAnkiGünData
        {
            get => şuankigünData;

            set
            {
                if (şuankigünData != value)
                {
                    şuankigünData = value;
                    OnPropertyChanged(nameof(ŞuAnkiGünData));
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

        public ICommand ResimGör { get; }

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

        public ICommand VeriAra { get; }
        public ObservableCollection<Data> YaklaşanEtkinlikler
        {
            get => yaklaşanEtkinlikler;

            set
            {
                if (yaklaşanEtkinlikler != value)
                {
                    yaklaşanEtkinlikler = value;
                    OnPropertyChanged(nameof(YaklaşanEtkinlikler));
                }
            }
        }

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
            AyGünler = new ObservableCollection<Data>(Günler?.Where(z => z.TamTarih.Month == SeçiliAy));
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
                SetRegistryValue(BaşlangıçtaÇalışacak);
            }

            void SaveColumnRowSettings()
            {
                Properties.Settings.Default.Satır = SatırSayısı;
                Properties.Settings.Default.Sütün = SütünSayısı;
                Properties.Settings.Default.Save();
            }
        }

        private void Properties_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "KontrolSüresi" || e.PropertyName == "PopupSüresi" || e.PropertyName == "HaftaSonlarıGizle")
            {
                Properties.Settings.Default.Save();
            }
        }
        private void SetRegistryValue(bool isChecked)
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
                MessageBox.Show(ex.Message, "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private ObservableCollection<Data> TakvimVerileriniOluştur(short SeçiliYıl)
        {
            XmlNodeList xmlNodeList = xmlDataProvider.Document?.SelectNodes("/Veriler/Veri");
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

                        if (xmlNodeList != null)
                        {
                            foreach (XmlNode xn in from XmlNode xn in xmlNodeList where DateTime.Parse(xn["Gun"]?.InnerText) == data.TamTarih select xn)
                            {
                                data.Id = Convert.ToInt32(xn.Attributes.GetNamedItem("Id").Value);
                            }
                        }
                        Günler.Add(data);
                    }
                }
            }

            return Günler;
        }

        private ObservableCollection<Data> YaklaşanEtkinlikleriAl()
        {
            YaklaşanEtkinlikler = new ObservableCollection<Data>();
            if (xmlDataProvider.Data is ICollection<XmlNode> xmlNodeCollection)
            {
                foreach (XmlNode xmlnode in xmlNodeCollection)
                {
                    _ = DateTime.TryParseExact(xmlnode.Attributes.GetNamedItem("SaatBaslangic").Value, "H:m", new CultureInfo("tr-TR"), DateTimeStyles.None, out DateTime saat);
                    bool yaklaşanetkinlik = DateTime.Parse(xmlnode["Gun"]?.InnerText) == DateTime.Today && saat > DateTime.Now && saat.AddHours(-1) < DateTime.Now;
                    bool tekraretkinlik = DateTime.Today.Day == DateTime.Parse(xmlnode["Gun"]?.InnerText).Day && xmlnode.Attributes.GetNamedItem("AyTekrar")?.Value == "true" && saat > DateTime.Now && saat.AddHours(-1) < DateTime.Now;
                    if (yaklaşanetkinlik || tekraretkinlik)
                    {
                        Data data = new Data
                        {
                            GünNotAçıklama = xmlnode["Aciklama"]?.InnerText,
                            TamTarih = saat
                        };
                        if (xmlnode["Resim"] != null)
                        {
                            data.ResimData = Convert.FromBase64String(xmlnode["Resim"]?.InnerText);
                        }
                        YaklaşanEtkinlikler.Add(data);
                    }
                }
            }
            return YaklaşanEtkinlikler;
        }

        private void WriteXmlRootData(string xmlfilepath)
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
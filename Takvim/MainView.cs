using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Winforms = System.Windows.Forms;

namespace Takvim
{
    public partial class MainViewModel
    {
        public static readonly string xmldatasavefolder = Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);

        public static readonly string xmlpath = Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath) + @"\Data.xml";

        public static Winforms.NotifyIcon AppNotifyIcon;

        public static WindowState AppWindowState = WindowState.Maximized;

        public static Window duyurularwindow;

        public static XmlDataProvider xmlDataProvider;

        private static SpeechSynthesizer synthesizer;

        private readonly CollectionViewSource Cvs = (CollectionViewSource)Application.Current?.MainWindow?.TryFindResource("Cvs");

        private readonly CollectionViewSource FilteredCvs = (CollectionViewSource)Application.Current?.MainWindow?.TryFindResource("FilteredCvs");

        private DateTime? animasyonTarih;

        private string aramaMetin;

        private ObservableCollection<Data> ayGünler;

        private bool başlangıçtaÇalışacak = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true).GetValue("Takvim") != null;

        private Brush bayramTatilRenk = Properties.Settings.Default.BayramRenk.ConvertToBrush();

        private string etkinlik;

        private Brush gövdeRenk = Properties.Settings.Default.GövdeRenk.ConvertToBrush();

        private ObservableCollection<Data> günler;

        private Brush resmiTatilRenk = Properties.Settings.Default.ResmiTatil.ConvertToBrush();

        private short satırSayısı = Properties.Settings.Default.Satır;

        private short seçiliAy = (short)DateTime.Now.Month;

        private Brush seçiliRenkCmt = Properties.Settings.Default.CmtRenk.ConvertToBrush();

        private Brush seçiliRenkPaz = Properties.Settings.Default.PazRenk.ConvertToBrush();

        private short seçiliYıl = (short)DateTime.Now.Year;

        private short sütünSayısı = Properties.Settings.Default.Sütün;

        private bool şuAnkiAy;

        private Data şuAnkiGünVerisi;

        private bool tümkayıtlar = true;

        private ObservableCollection<Data> üçAylıkGünler;

        private ObservableCollection<Data> yaklaşanEtkinlikler;

        private string zaman;

        private DateTime şuAnkiGün = DateTime.Today;

        private string seçiliTts;

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

        public DateTime ŞuAnkiGün
        {
            get => şuAnkiGün;

            set
            {
                if (şuAnkiGün != value)
                {
                    şuAnkiGün = value;
                    OnPropertyChanged(nameof(ŞuAnkiGün));
                }
            }
        }

        public int SeçiliGün { get; set; } = DateTime.Now.Day - 1;

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

        public bool ŞuAnkiAy
        {
            get => şuAnkiAy;

            set
            {
                if (şuAnkiAy != value)
                {
                    şuAnkiAy = value;
                    OnPropertyChanged(nameof(ŞuAnkiAy));
                }
            }
        }

        public Data ŞuAnkiGünVerisi
        {
            get => şuAnkiGünVerisi;

            set
            {
                if (şuAnkiGünVerisi != value)
                {
                    şuAnkiGünVerisi = value;
                    OnPropertyChanged(nameof(ŞuAnkiGünVerisi));
                }
            }
        }

        public bool TümKayıtlar
        {
            get => tümkayıtlar;

            set
            {
                if (tümkayıtlar != value)
                {
                    tümkayıtlar = value;
                    OnPropertyChanged(nameof(TümKayıtlar));
                }
            }
        }

        public ObservableCollection<Data> ÜçAylıkGünler
        {
            get => üçAylıkGünler;

            set
            {
                if (üçAylıkGünler != value)
                {
                    üçAylıkGünler = value;
                    OnPropertyChanged(nameof(ÜçAylıkGünler));
                }
            }
        }

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

        public IEnumerable<string> TtsDilleri { get; set; } = new List<string>();

        public string SeçiliTts
        {
            get => seçiliTts;

            set
            {
                if (seçiliTts != value)
                {
                    seçiliTts = value;
                    OnPropertyChanged(nameof(SeçiliTts));
                }
            }
        }

        public string Zaman
        {
            get => zaman;

            set
            {
                if (zaman != value)
                {
                    zaman = value;
                    OnPropertyChanged(nameof(Zaman));
                }
            }
        }
    }
}
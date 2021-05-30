using Extensions;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using Takvim.Properties;
using Winforms = System.Windows.Forms;

namespace Takvim
{
    public partial class MainViewModel : InpcBase, IDataErrorInfo
    {
        static MainViewModel()
        {
            if (FilteredCvs is not null)
            {
                FilteredCvs.Filter += (s, e) => e.Accepted = DateTime.Parse((e.Item as XmlNode)?["Gun"]?.InnerText) == DateTime.Today;
            }
        }

        public MainViewModel()
        {
            GenerateSystemTrayMenu();
            DatetimeTimer();
            GetTtsLang();
            WriteXmlRootData(xmlpath);

            xmlDataProvider = (XmlDataProvider)Application.Current?.TryFindResource("XmlData");

            xmlDataProvider.Source = new Uri(xmlpath);

            TakvimVerileriniOluştur(SeçiliYıl);

            AyTakvimVerileriniOluştur(SeçiliAy);

            YılGeri = new RelayCommand<object>(parameter => SeçiliYıl--, parameter => SeçiliYıl > 1);

            AyGeri = new RelayCommand<object>(parameter => SeçiliAy--, parameter => SeçiliAy > 1);

            Yılİleri = new RelayCommand<object>(parameter => SeçiliYıl++, parameter => SeçiliYıl < 9999);

            Ayİleri = new RelayCommand<object>(parameter => SeçiliAy++, parameter => SeçiliAy < 12);

            Günİleri = new RelayCommand<object>(parameter => ŞuAnkiGün = ŞuAnkiGün.AddDays(1), parameter => ŞuAnkiGün < DateTime.Parse($"31/12/{DateTime.Now.Year}"));

            GünBugün = new RelayCommand<object>(parameter => ŞuAnkiGün = DateTime.Today);

            GünGeri = new RelayCommand<object>(parameter => ŞuAnkiGün = ŞuAnkiGün.AddDays(-1), parameter => ŞuAnkiGün > DateTime.Parse($"1/1/{DateTime.Now.Year}"));

            SatırSütünSıfırla = new RelayCommand<object>(parameter =>
            {
                SatırSayısı = 3;
                SütünSayısı = 4;
                Settings.Default.Save();
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

            AyarSıfırla = new RelayCommand<object>(parameter =>
            {
                Settings.Default.Reset();
                MessageBox.Show("Ayarlar Varsayılana Çevrildi. Yeniden Başlatın.", "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }, parameter => true);

            EskiVerileriSil = new RelayCommand<object>(parameter =>
            {
                if (MessageBox.Show($"{SeçiliYıl} yılına ait tüm kayıtları silmek istiyor musun? Dikkat bu işlem geri alınamaz.", "TAKVİM", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    XDocument doc = XDocument.Load(xmlpath);
                    doc.Root.Elements("Veri").Where(z => DateTime.Parse(z.Element("Gun").Value).Year == SeçiliYıl).Remove();
                    doc.Save(xmlpath);
                    xmlDataProvider.Refresh();
                }
            }, parameter => SeçiliYıl < DateTime.Now.Year && File.Exists(xmlpath));

            DuyurularPopupEkranıAç = new RelayCommand<object>(parameter =>
            {
                duyurularwindow = new Window
                {
                    Content = new FloatingWindowControl(),
                    DataContext = this,
                    Width = 224,
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true,
                    Height = 154,
                    ShowInTaskbar = false,
                    Topmost = true,
                    Background = Brushes.Transparent,
                    Top = SystemParameters.PrimaryScreenHeight - 200,
                    Left = SystemParameters.PrimaryScreenWidth - 250,
                };
                YaklaşanEtkinlikleriAl();
                if (YaklaşanEtkinlikler.Any())
                {
                    if (Settings.Default.YaklaşanEtkinlikleriOku)
                    {
                        ListeyiOku(YaklaşanEtkinlikler.Select(z => z.GünNotAçıklama));
                    }
                    else
                    {
                        SystemSounds.Exclamation.Play();
                    }
                    duyurularwindow.Show();
                }
            }, parameter => true);

            VeriAra = new RelayCommand<object>(parameter =>
            {
                Cvs.Filter += (s, e) =>
                {
                    if (e.Item is XmlNode node)
                    {
                        e.Accepted = TümKayıtlar
                            ? node?["Aciklama"]?.InnerText.Contains(AramaMetin, StringComparison.OrdinalIgnoreCase) == true || (e.Item as XmlNode)?.Attributes.GetNamedItem("Ocr")?.InnerText.Contains(AramaMetin, StringComparison.OrdinalIgnoreCase) == true
                            : DateTime.Parse(node?["Gun"]?.InnerText) > DateTime.Today && node?["Aciklama"]?.InnerText.Contains(AramaMetin) == true || (e.Item as XmlNode)?.Attributes.GetNamedItem("Ocr")?.InnerText.Contains(AramaMetin, StringComparison.OrdinalIgnoreCase) == true;
                    }
                };
            }, parameter => !string.IsNullOrWhiteSpace(AramaMetin));

            WebAdreseGit = new RelayCommand<object>(parameter => Process.Start(parameter as string), parameter => true);

            MetinOku = new RelayCommand<object>(parameter =>
            {
                if (parameter is string metin && CheckTtsSelected())
                {
                    synthesizer.SelectVoice(Settings.Default.SeçiliTts);
                    synthesizer.SpeakAsync(metin);
                }
            }, parameter => true);

            VeriOku = new RelayCommand<object>(parameter => ListeyiOku(FilteredCvs.View.SourceCollection.OfType<XmlNode>().Where(z => DateTime.Parse(z["Gun"]?.InnerText) == ŞuAnkiGün).Select(z => z["Aciklama"].InnerText)), parameter => true);

            VeritabanıAç = new RelayCommand<object>(parameter =>
            {
                if (MessageBox.Show("Veritabanı dosyasını düzenlemek istiyor musun? Dikkat yanlış düzenleme programın açılmamasına neden olabilir. Devam edilsin mi?", "TAKVİM", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    Process.Start(xmlpath);
                }
            }, parameter => true);

            ŞuAnkiGünVerisi = Günler.FirstOrDefault(z => z.TamTarih == ŞuAnkiGün);

            PropertyChanged += MainViewModel_PropertyChanged;

            Settings.Default.PropertyChanged += Properties_PropertyChanged;
        }

        public ICommand AyarSıfırla { get; }

        public ICommand AyGeri { get; }

        public ICommand Ayİleri { get; }

        public ICommand DuyurularPopupEkranıAç { get; }

        public string Error => string.Empty;

        public ICommand EskiVerileriSil { get; }

        public ICommand GünBugün { get; }

        public ICommand GünGeri { get; }

        public ICommand Günİleri { get; }

        public ICommand MetinOku { get; }

        public ICommand SatırSütünSıfırla { get; }

        public ICommand VeriAra { get; }

        public ICommand VeriOku { get; }

        public ICommand VeritabanıAç { get; }

        public ICommand WebAdreseGit { get; }

        public ICommand YılaGit { get; }

        public ICommand YılGeri { get; }

        public ICommand Yılİleri { get; }

        public string this[string columnName] => columnName switch
        {
            "SeçiliYıl" when SeçiliYıl is <= 0 or > 9999 => "Seçili Yıl 1-9999 Aralığındadır.",
            _ => null
        };

        private void AyTakvimVerileriniOluştur(short SeçiliAy)
        {
            ÜçAylıkGünler = new ObservableCollection<Data>(Günler?.Where(z => z.TamTarih.Month == SeçiliAy - 1 || z.TamTarih.Month == SeçiliAy || z.TamTarih.Month == SeçiliAy + 1));
            AyGünler = new ObservableCollection<Data>(ÜçAylıkGünler.Where(z => z.TamTarih.Month == SeçiliAy));
        }

        private bool CheckTtsSelected() => !string.IsNullOrEmpty(Settings.Default.SeçiliTts);

        private void DatetimeTimer()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                DispatcherTimer datetimer = new(DispatcherPriority.Normal)
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                datetimer.Start();
                datetimer.Tick += (s, e) => Zaman = DateTime.Now.ToString("HH:mm:ss");
            }
            Zaman = DateTime.Now.ToString("HH:mm:ss");
        }

        private void GenerateSystemTrayMenu()
        {
            Winforms.ContextMenu contextmenu = new();
            Winforms.MenuItem menuitem = new()
            {
                Index = 0,
                Text = "VERİ EKLE"
            };
            contextmenu.MenuItems.Add(menuitem);
            AppNotifyIcon = new Winforms.NotifyIcon
            {
                BalloonTipText = "Uygulama Sistem Tepsisine Gönderildi.",
                BalloonTipTitle = "TAKVİM",
                Text = "Takvim",
                Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/Takvim;component/icon.ico")).Stream),
                ContextMenu = contextmenu
            };

            menuitem.Click += (s, e) => AddData();

            AppNotifyIcon.MouseClick += (s, e) =>
            {
                Application.Current.MainWindow?.Show();
                Application.Current.MainWindow.WindowState = AppWindowState;
            };

            static void AddData()
            {
                Data data = new()
                {
                    TamTarih = DateTime.Today
                };
                data.VeriEkleEkranı.Execute(data);
            }
        }

        private void GetTtsLang() => TtsDilleri = synthesizer.GetInstalledVoices().Select(z => z.VoiceInfo.Name);

        private void ListeyiOku(IEnumerable<string> Veri)
        {
            if (CheckTtsSelected())
            {
                try
                {
                    synthesizer.SelectVoice(Settings.Default.SeçiliTts);
                    foreach (string item in Veri)
                    {
                        synthesizer.SpeakAsync(item);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("Önce TTS Seçimi Yapın.", "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is "SütünSayısı")
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

            if (e.PropertyName is "SatırSayısı")
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

            if (e.PropertyName is "SeçiliYıl" && SeçiliYıl > 0 && SeçiliYıl < 10000)
            {
                TakvimVerileriniOluştur(SeçiliYıl);
            }

            if (e.PropertyName is "SeçiliAy" && SeçiliAy > 0 && SeçiliAy < 13)
            {
                AyTakvimVerileriniOluştur(SeçiliAy);
            }

            if (e.PropertyName is "SeçiliRenkPaz" or "GövdeRenk" or "SeçiliRenkCmt" or "ResmiTatilRenk" or "BayramTatilRenk")
            {
                Settings.Default.PazRenk = SeçiliRenkPaz.ConvertToColor();
                Settings.Default.CmtRenk = SeçiliRenkCmt.ConvertToColor();
                Settings.Default.ResmiTatil = ResmiTatilRenk.ConvertToColor();
                Settings.Default.GövdeRenk = GövdeRenk.ConvertToColor();
                Settings.Default.BayramRenk = BayramTatilRenk.ConvertToColor();
                Settings.Default.Save();
            }

            if (e.PropertyName is "AramaMetin" && string.IsNullOrWhiteSpace(AramaMetin))
            {
                Cvs.View.Filter = null;
            }

            if (e.PropertyName is "BaşlangıçtaÇalışacak")
            {
                SetRegistryValue(BaşlangıçtaÇalışacak);
            }

            if (e.PropertyName is "ŞuAnkiGün")
            {
                FilteredCvs.Filter += (s, e) => e.Accepted = DateTime.Parse((e.Item as XmlNode)?["Gun"]?.InnerText) == ŞuAnkiGün;
                ŞuAnkiGünVerisi = Günler.FirstOrDefault(z => z.TamTarih == ŞuAnkiGün);
            }

            void SaveColumnRowSettings()
            {
                Settings.Default.Satır = SatırSayısı;
                Settings.Default.Sütün = SütünSayısı;
                Settings.Default.Save();
            }
        }

        private void Properties_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is "SeçiliTts" or "KontrolSüresi" or "PopupSüresi" or "MiniTakvimAçık" or "HaftaSonlarıGizle" or "UyarıSaatSüresi" or "VarsayılanTakvim" or "AyarlarGörünür" or "Panel" or "YatayAdetOranı")
            {
                Settings.Default.Save();
            }

            if (e.PropertyName is "YaklaşanEtkinlikleriOku" && !string.IsNullOrEmpty(Settings.Default.SeçiliTts))
            {
                Settings.Default.Save();
            }

            if (e.PropertyName is "Panel" && Settings.Default.Panel)
            {
                Settings.Default.YatayAdetOranı = (short)(SystemParameters.PrimaryScreenWidth / 250);
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
                        Data data = new()
                        {
                            GünAdı = date.ToString("ddd"),
                            Gün = (short)date.Day,
                            Ay = date.ToString("MMMM"),
                            Offset = (short)date.DayOfWeek,
                            TamTarih = date
                        };
                        if (xmlNodeList != null)
                        {
                            using IEnumerator<XmlNode> en = (from XmlNode xn in xmlNodeList where DateTime.Parse(xn["Gun"]?.InnerText) == data.TamTarih select xn).GetEnumerator();
                            while (en.MoveNext())
                            {
                                data.VeriSayısı++;
                            }
                        }
                        Günler.Add(data);
                    }
                }
            }

            return Günler;
        }

        private void WriteXmlRootData(string xmlfilepath)
        {
            if (!Directory.Exists(xmldatasavefolder))
            {
                Directory.CreateDirectory(xmldatasavefolder);
            }
            if (!File.Exists(xmlfilepath))
            {
                using XmlWriter writer = XmlWriter.Create(xmlpath);
                writer.WriteStartElement("Veriler");
                writer.WriteEndElement();
                writer.Flush();
            }
        }

        private ObservableCollection<Data> YaklaşanEtkinlikleriAl()
        {
            YaklaşanEtkinlikler = new ObservableCollection<Data>();
            if (xmlDataProvider.Data is ICollection<XmlNode> xmlNodeCollection)
            {
                foreach (XmlNode xmlnode in xmlNodeCollection)
                {
                    _ = DateTime.TryParseExact(xmlnode.Attributes.GetNamedItem("SaatBaslangic").Value, "H:m", new CultureInfo("tr-TR"), DateTimeStyles.None, out DateTime saat);
                    bool yaklaşanetkinlik = DateTime.Today.Day == DateTime.Parse(xmlnode["Gun"]?.InnerText).Day && saat > DateTime.Now && saat.AddHours(-Settings.Default.UyarıSaatSüresi) < DateTime.Now && xmlnode.Attributes.GetNamedItem("Okundu")?.Value != "true";
                    bool tekraretkinlik = DateTime.Today.Day == DateTime.Parse(xmlnode["Gun"]?.InnerText).Day && xmlnode.Attributes.GetNamedItem("AyTekrar")?.Value == "true" && saat > DateTime.Now && saat.AddHours(-Settings.Default.UyarıSaatSüresi) < DateTime.Now;
                    if (yaklaşanetkinlik || tekraretkinlik)
                    {
                        Data data = new()
                        {
                            GünNotAçıklama = xmlnode["Aciklama"]?.InnerText,
                            TamTarih = saat,
                            Id = Convert.ToInt32(xmlnode.Attributes.GetNamedItem("Id")?.Value)
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
    }
}
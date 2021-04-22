using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Takvim
{
    public partial class Data
    {
        private string ay;

        private bool ayTekrar;

        private double ayTekrarGun;

        private double boyut;

        private ObservableCollection<string> dosyalar;

        private string dosyaUzantı;

        private double etkinlikSüresi;

        private short gün;

        private string günAdı;

        private string günNotAçıklama;

        private int ıd;

        private bool kilitliMi;

        private string ocrMetin;

        private bool ocrSürüyor;

        private Task OcrTask;

        private short offset;

        private bool önemliMi;

        private byte[] pdfData;

        private byte[] resimData;

        private string saatBaşlangıç;

        private DateTime tamTarih;

        private Brush veriRenk;

        private int veriSayısı;

        private int webpQuality = 20;

        private string resimYolu;

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

        public bool AyTekrar
        {
            get => ayTekrar;

            set
            {
                if (ayTekrar != value)
                {
                    ayTekrar = value;
                    OnPropertyChanged(nameof(AyTekrar));
                }
            }
        }

        public double AyTekrarGun
        {
            get => ayTekrarGun;

            set
            {
                if (ayTekrarGun != value)
                {
                    ayTekrarGun = value;
                    OnPropertyChanged(nameof(AyTekrarGun));
                }
            }
        }

        public double Boyut
        {
            get => boyut;

            set
            {
                if (boyut != value)
                {
                    boyut = value;
                    OnPropertyChanged(nameof(Boyut));
                }
            }
        }

        public ObservableCollection<string> Dosyalar
        {
            get => dosyalar;

            set
            {
                if (dosyalar != value)
                {
                    dosyalar = value;
                    OnPropertyChanged(nameof(Dosyalar));
                }
            }
        }

        public string DosyaUzantı
        {
            get => dosyaUzantı;

            set
            {
                if (dosyaUzantı != value)
                {
                    dosyaUzantı = value;
                    OnPropertyChanged(nameof(DosyaUzantı));
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

        public short Gün
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

        public int Id
        {
            get => ıd;

            set
            {
                if (ıd != value)
                {
                    ıd = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public bool KilitliMi
        {
            get => kilitliMi;

            set
            {
                if (kilitliMi != value)
                {
                    kilitliMi = value;
                    OnPropertyChanged(nameof(KilitliMi));
                }
            }
        }

        public string OcrMetin
        {
            get => ocrMetin;

            set
            {
                if (ocrMetin != value)
                {
                    ocrMetin = value;
                    OnPropertyChanged(nameof(OcrMetin));
                }
            }
        }

        public bool OcrSürüyor
        {
            get => ocrSürüyor;

            set
            {
                if (ocrSürüyor != value)
                {
                    ocrSürüyor = value;
                    OnPropertyChanged(nameof(OcrSürüyor));
                }
            }
        }

        public short Offset
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

        public byte[] PdfData
        {
            get => pdfData;

            set
            {
                if (pdfData != value)
                {
                    pdfData = value;
                    OnPropertyChanged(nameof(PdfData));
                }
            }
        }

        public byte[] ResimData
        {
            get => resimData;

            set
            {
                if (resimData != value)
                {
                    resimData = value;
                    OnPropertyChanged(nameof(ResimData));
                }
            }
        }

        public string ResimYolu
        {
            get => resimYolu;

            set
            {
                if (resimYolu != value)
                {
                    resimYolu = value;
                    OnPropertyChanged(nameof(ResimYolu));
                }
            }
        }

        public string SaatBaşlangıç
        {
            get => saatBaşlangıç;

            set
            {
                if (saatBaşlangıç != value)
                {
                    saatBaşlangıç = value;
                    OnPropertyChanged(nameof(SaatBaşlangıç));
                }
            }
        }

        public DateTime TamTarih
        {
            get => tamTarih;

            set
            {
                if (tamTarih != value)
                {
                    tamTarih = value;
                    OnPropertyChanged(nameof(TamTarih));
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

        public int WebpQuality
        {
            get => webpQuality;

            set
            {
                if (webpQuality != value)
                {
                    webpQuality = value;
                    OnPropertyChanged(nameof(WebpQuality));
                }
            }
        }
    }
}
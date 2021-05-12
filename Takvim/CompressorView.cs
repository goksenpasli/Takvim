using System.Collections.ObjectModel;

namespace Takvim
{
    public class ArchiveData : InpcBase
    {
        private long boyut;

        private string dosyaAdı;

        public long Boyut
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

        public string DosyaAdı
        {
            get => dosyaAdı;

            set
            {
                if (dosyaAdı != value)
                {
                    dosyaAdı = value;
                    OnPropertyChanged(nameof(DosyaAdı));
                }
            }
        }
    }

    public class CompressorView : Data
    {
        private string arşivDosyaYolu;

        private ObservableCollection<ArchiveData> arşivİçerik;

        private int biçim;

        private string dosyaAdı;

        private string kayıtYolu;

        private double oran;

        private int sıkıştırmaDerecesi = 5;

        private bool sürüyor;

        private string veriGirişKayıtYolu;

        public string ArşivDosyaYolu
        {
            get { return arşivDosyaYolu; }

            set
            {
                if (arşivDosyaYolu != value)
                {
                    arşivDosyaYolu = value;
                    OnPropertyChanged(nameof(ArşivDosyaYolu));
                }
            }
        }

        public ObservableCollection<ArchiveData> Arşivİçerik
        {
            get => arşivİçerik;

            set
            {
                if (arşivİçerik != value)
                {
                    arşivİçerik = value;
                    OnPropertyChanged(nameof(Arşivİçerik));
                }
            }
        }

        public int Biçim
        {
            get => biçim;

            set
            {
                if (biçim != value)
                {
                    biçim = value;
                    OnPropertyChanged(nameof(Biçim));
                }
            }
        }

        public string DosyaAdı
        {
            get => dosyaAdı;

            set
            {
                if (dosyaAdı != value)
                {
                    dosyaAdı = value;
                    OnPropertyChanged(nameof(DosyaAdı));
                }
            }
        }

        public string KayıtYolu
        {
            get => kayıtYolu;

            set
            {
                if (kayıtYolu != value)
                {
                    kayıtYolu = value;
                    OnPropertyChanged(nameof(KayıtYolu));
                    OnPropertyChanged(nameof(Biçim));
                }
            }
        }

        public double Oran
        {
            get => oran;

            set
            {
                if (oran != value)
                {
                    oran = value;
                    OnPropertyChanged(nameof(Oran));
                }
            }
        }

        public int SıkıştırmaDerecesi
        {
            get => sıkıştırmaDerecesi;

            set
            {
                if (sıkıştırmaDerecesi != value)
                {
                    sıkıştırmaDerecesi = value;
                    OnPropertyChanged(nameof(SıkıştırmaDerecesi));
                }
            }
        }

        public bool Sürüyor
        {
            get => sürüyor;

            set
            {
                if (sürüyor != value)
                {
                    sürüyor = value;
                    OnPropertyChanged(nameof(Sürüyor));
                }
            }
        }

        public string VeriGirişKayıtYolu
        {
            get => veriGirişKayıtYolu;

            set
            {
                if (veriGirişKayıtYolu != value)
                {
                    veriGirişKayıtYolu = value;
                    OnPropertyChanged(nameof(VeriGirişKayıtYolu));
                }
            }
        }
    }
}
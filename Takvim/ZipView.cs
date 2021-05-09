namespace Takvim
{
    public class ZipView : Data
    {
        private int biçim;

        private string dosyaAdı;

        private string kayıtYolu;

        private double oran;

        private int sıkıştırmaDerecesi = 5;

        private bool sürüyor;

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
    }
}

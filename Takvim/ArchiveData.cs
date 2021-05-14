namespace Takvim
{
    public class ArchiveData : InpcBase
    {
        private long boyut;

        private string dosyaAdı;

        private string crc;

        private long sıkıştırılmışBoyut;

        private double oran;
        private string uzantı;

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

        public long SıkıştırılmışBoyut
        {
            get => sıkıştırılmışBoyut;
            set
            {
                if (sıkıştırılmışBoyut != value)
                {
                    sıkıştırılmışBoyut = value;
                    OnPropertyChanged(nameof(SıkıştırılmışBoyut));
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

        public string Uzantı
        {
            get => uzantı;
            set
            {
                if (uzantı != value)
                {
                    uzantı = value;
                    OnPropertyChanged(nameof(Uzantı));
                }
            }
        }

        public string Crc
        {
            get => crc;

            set
            {
                if (crc != value)
                {
                    crc = value;
                    OnPropertyChanged(nameof(Crc));
                }
            }
        }
    }
}
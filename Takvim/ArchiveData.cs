namespace Takvim
{
    public class ArchiveData : InpcBase
    {
        private long boyut;

        private string dosyaAdı;

        private long crc;

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

        public long Crc
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
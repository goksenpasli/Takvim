using Extensions;
using System;

namespace Takvim
{
    public class ArchiveData : InpcBase
    {
        private long boyut;

        private string dosyaAdı;

        private string crc;

        private long sıkıştırılmışBoyut;

        private double oran;

        private DateTime? düzenlenmeZamanı;

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

        public DateTime? DüzenlenmeZamanı
        {
            get => düzenlenmeZamanı;
            set
            {
                if (düzenlenmeZamanı != value)
                {
                    düzenlenmeZamanı = value;
                    OnPropertyChanged(nameof(DüzenlenmeZamanı));
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
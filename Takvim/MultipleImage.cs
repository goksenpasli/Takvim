using System;
using PdfSharp.Windows;

namespace Takvim
{
    public class MultipleImage : InpcBase
    {
        private double angle;
        private double zoom = 1;
        private string resim;

        public double Angle
        {
            get => angle;
            set
            {
                if (angle != value)
                {
                    angle = value;
                    OnPropertyChanged(nameof(Angle));
                }
            }
        }

        public string Resim
        {
            get => resim;

            set
            {
                if (resim != value)
                {
                    resim = value;
                    OnPropertyChanged(nameof(Resim));
                }
            }
        }

        public double Zoom
        {
            get => zoom;

            set
            {
                if (zoom != value)
                {
                    zoom = value;
                    OnPropertyChanged(nameof(Zoom));
                }
            }
        }
    }
}
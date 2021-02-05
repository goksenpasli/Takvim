namespace Takvim
{
    public class ImageViewer : InpcBase
    {
        private double angle;

        private double zoom = 1;
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

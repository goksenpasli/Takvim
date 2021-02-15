using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Takvim
{
    public class ImageViewer : InpcBase
    {
        public ICommand Yazdır { get; }

        public ImageViewer()
        {
        }

        public ImageViewer(ImageSource imageSource) : this()
        {
            Yazdır = new RelayCommand<object>(parameter =>
            {
                PrintDialog pd = new PrintDialog();
                DrawingVisual dv = new DrawingVisual();
                if (pd.ShowDialog() == true)
                {
                    using (DrawingContext dc = dv.RenderOpen())
                    {
                        BitmapSource imagesource = imageSource.Width > imageSource.Height
                            ? TwainControl.Picture.Resize((BitmapSource)imageSource, pd.PrintableAreaHeight, pd.PrintableAreaWidth, 90, 300, 300)
                            : TwainControl.Picture.Resize((BitmapSource)imageSource, pd.PrintableAreaWidth, pd.PrintableAreaHeight, 0, 300, 300);
                        imagesource.Freeze();
                        dc.DrawImage(imagesource, new Rect(0, 0, pd.PrintableAreaWidth, pd.PrintableAreaHeight));
                    }
                    pd.PrintVisual(dv, "");
                    imageSource = null;
                }
            }, parameter => true);
        }

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
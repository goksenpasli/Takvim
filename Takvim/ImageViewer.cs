using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Takvim
{
    public class ImageViewer : InpcBase
    {
        public ICommand Yazdır { get; }

        public ImageViewer()
        {
            Yazdır = new RelayCommand<object>(parameter =>
            {
                if (parameter is BitmapSource imageSource)
                {
                    PrintDialog pd = new PrintDialog();
                    DrawingVisual dv = new DrawingVisual();
                    if (pd.ShowDialog() == true)
                    {
                        using (DrawingContext dc = dv.RenderOpen())
                        {
                            BitmapSource imagesource = imageSource.Width > imageSource.Height
                                ? TwainControl.Picture.Resize(imageSource, pd.PrintableAreaHeight, pd.PrintableAreaWidth, 90, 300, 300)
                                : TwainControl.Picture.Resize(imageSource, pd.PrintableAreaWidth, pd.PrintableAreaHeight, 0, 300, 300);
                            imagesource.Freeze();
                            dc.DrawImage(imagesource, new Rect(0, 0, pd.PrintableAreaWidth, pd.PrintableAreaHeight));
                        }
                        pd.PrintVisual(dv, "");
                    }
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
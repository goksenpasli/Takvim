using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Takvim
{
    public class ImageViewer :Data
    {
        private double angle;

        private BitmapSource resim;

        private double zoom = 1;

        public ImageViewer(XmlElement xmldata)
        {
            Xmldata = xmldata;
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

            OcrUygula = new RelayCommand<object>(parameter =>
            {
                Task.Factory.StartNew(() =>
                {
                    OcrSürüyor = true;
                    OcrMetin = Resim.ToTiffJpegByteArray(ExtensionMethods.Format.Jpg).OcrYap();
                    OcrSürüyor = false;
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        xmldata.SetAttribute("Ocr", OcrMetin);
                        MainViewModel.xmlDataProvider.Document.Save(MainViewModel.xmlpath);
                    }));
                }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            }, parameter => Environment.OSVersion.Version.Major > 5 && Directory.Exists(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\tessdata"));

            Resim = (BitmapSource)new Base64ImageConverter().Convert(xmldata["Resim"].InnerText, null, null, CultureInfo.CurrentCulture);
            OcrMetin = xmldata.GetAttribute("Ocr");
        }

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

        public new ICommand OcrUygula { get; }

        public BitmapSource Resim
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

        public ICommand Yazdır { get; }
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

        public XmlElement Xmldata { get; set; }
    }
}
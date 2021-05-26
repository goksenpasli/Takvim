using System;
using System.Globalization;
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
    public class ImageViewer : Data
    {
        private readonly Base64Converter base64Converter = new();

        private readonly Base64ImageConverter base64ImageConverter = new();

        private double angle;

        private Visibility ımgTabVisible;

        private int ındex;

        private Visibility pdfTabVisible;

        private BitmapSource resim;

        private double zoom = 0.25;

        public ImageViewer(XmlElement xmldata)
        {
            Xmldata = xmldata;
            Yazdır = new RelayCommand<object>(parameter =>
            {
                if (parameter is BitmapSource imageSource)
                {
                    PrintDialog pd = new();
                    DrawingVisual dv = new();
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
            }, parameter => Resim is not null);

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
            }, parameter => Resim is not null && Environment.OSVersion.Version.Major > 5 && Ocr.tesseractexsist);

            PdfData = (byte[])base64Converter.Convert(xmldata["Pdf"]?.InnerText, null, null, CultureInfo.CurrentCulture);
            Resim = (BitmapSource)base64ImageConverter.Convert(xmldata["Resim"]?.InnerText, null, null, CultureInfo.CurrentCulture);
            OcrMetin = xmldata.GetAttribute("Ocr");

            if (PdfData is null && Resim is not null)
            {
                Index = 0;
                PdfTabVisible = Visibility.Collapsed;
            }

            if (PdfData is not null && Resim is null)
            {
                Index = 1;
                ImgTabVisible = Visibility.Collapsed;
            }

            if (PdfData is null && Resim is null)
            {
                Index = 2;
                PdfTabVisible = Visibility.Collapsed;
                ImgTabVisible = Visibility.Collapsed;
            }
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

        public Visibility ImgTabVisible
        {
            get => ımgTabVisible;

            set
            {
                if (ımgTabVisible != value)
                {
                    ımgTabVisible = value;
                    OnPropertyChanged(nameof(ImgTabVisible));
                }
            }
        }

        public int Index
        {
            get => ındex;

            set
            {
                if (ındex != value)
                {
                    ındex = value;
                    OnPropertyChanged(nameof(Index));
                }
            }
        }

        public new ICommand OcrUygula { get; }

        public Visibility PdfTabVisible
        {
            get => pdfTabVisible;

            set
            {
                if (pdfTabVisible != value)
                {
                    pdfTabVisible = value;
                    OnPropertyChanged(nameof(PdfTabVisible));
                }
            }
        }

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

        public XmlElement Xmldata { get; }

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
    }
}
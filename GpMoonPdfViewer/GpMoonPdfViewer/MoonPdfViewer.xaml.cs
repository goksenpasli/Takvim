using Extensions;
using Microsoft.Win32;
using MoonPdfLib.MuPdf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GpMoonPdfViewer
{
    public partial class MoonPdfViewer : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty PdfDataFilePathProperty = DependencyProperty.Register("PdfDataFilePath", typeof(string), typeof(MoonPdfViewer), new PropertyMetadata(null, PdfDataFilePathChanged));

        public static readonly DependencyProperty PdfDataProperty = DependencyProperty.Register("PdfData", typeof(byte[]), typeof(MoonPdfViewer), new PropertyMetadata(null, PdfDataChanged));

        private Visibility acKaydetButtonEtkin;

        private Visibility contextMenuVisible;

        private bool controlsActive;

        private double customZoomLevel = 1;

        private Visibility printButtonEtkin;

        private ObservableCollection<int> sayfalar;

        private int şuankiSayfa;

        private Visibility toolBarVisible;

        private int toplamSayfa;

        public MoonPdfViewer()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility AcKaydetButtonEtkin
        {
            get => acKaydetButtonEtkin;

            set
            {
                if (acKaydetButtonEtkin != value)
                {
                    acKaydetButtonEtkin = value;
                    OnPropertyChanged(nameof(AcKaydetButtonEtkin));
                }
            }
        }

        public Visibility ContextMenuVisible
        {
            get => contextMenuVisible;

            set
            {
                if (contextMenuVisible != value)
                {
                    contextMenuVisible = value;
                    OnPropertyChanged(nameof(ContextMenuVisible));
                }
            }
        }

        public bool ControlsActive
        {
            get => controlsActive;

            set
            {
                if (controlsActive != value)
                {
                    controlsActive = value;
                    OnPropertyChanged(nameof(ControlsActive));
                }
            }
        }

        public double CustomZoomLevel
        {
            get => customZoomLevel;

            set
            {
                if (customZoomLevel != value)
                {
                    customZoomLevel = value;
                    OnPropertyChanged(nameof(CustomZoomLevel));
                }
            }
        }

        public byte[] PdfData { get => (byte[])GetValue(PdfDataProperty); set => SetValue(PdfDataProperty, value); }

        public string PdfDataFilePath { get => (string)GetValue(PdfDataFilePathProperty); set => SetValue(PdfDataFilePathProperty, value); }

        public Visibility PrintButtonEtkin
        {
            get => printButtonEtkin;

            set
            {
                if (printButtonEtkin != value)
                {
                    printButtonEtkin = value;
                    OnPropertyChanged(nameof(PrintButtonEtkin));
                }
            }
        }

        public ObservableCollection<int> Sayfalar
        {
            get => sayfalar;

            set
            {
                if (sayfalar != value)
                {
                    sayfalar = value;
                    OnPropertyChanged(nameof(Sayfalar));
                }
            }
        }

        public int ŞuankiSayfa
        {
            get => şuankiSayfa;

            set
            {
                if (şuankiSayfa != value)
                {
                    şuankiSayfa = value;
                    Mpp.GotoPage(şuankiSayfa);
                    OnPropertyChanged(nameof(ŞuankiSayfa));
                }
            }
        }

        public Visibility ToolBarVisible
        {
            get => toolBarVisible;

            set
            {
                if (toolBarVisible != value)
                {
                    toolBarVisible = value;
                    OnPropertyChanged(nameof(ToolBarVisible));
                }
            }
        }

        public int ToplamSayfa
        {
            get => toplamSayfa;

            set
            {
                if (toplamSayfa != value)
                {
                    toplamSayfa = value;
                    OnPropertyChanged(nameof(ToplamSayfa));
                }
            }
        }

        public static BitmapImage PdfExtractSmallPreviewImage(MoonPdfViewer moonPdfViewer, int sayfano, float zoom = 0.1f)
        {
            return Task.Factory.StartNew(() =>
            {
                using (Bitmap bmp = MuPdfWrapper.ExtractPage(moonPdfViewer.Mpp.CurrentSource, sayfano, zoom))
                {
                    return bmp.ToBitmapImage(ImageFormat.Jpeg);
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Result;
        }

        protected virtual void OnPropertyChanged(string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private static void PdfDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MoonPdfViewer pdfViewer && e.NewValue != null)
            {
                try
                {
                    if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                    {
                        return;
                    }

                    using (MemoryStream Ms = new MemoryStream(e.NewValue as byte[], false))
                    {
                        pdfViewer.Mpp?.Open(new MemorySource(Ms.ToArray()));
                    }
                    pdfViewer.Sayfalar = new ObservableCollection<int>(Enumerable.Range(1, pdfViewer.Mpp.TotalPages));
                    pdfViewer.ŞuankiSayfa = 1;
                    pdfViewer.ToplamSayfa = (int)(pdfViewer.Mpp?.TotalPages);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "EBYS", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private static void PdfDataFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MoonPdfViewer pdfViewer && e.NewValue != null)
            {
                try
                {
                    if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                    {
                        return;
                    }

                    string uriString = (string)e.NewValue;
                    pdfViewer.Mpp?.Open(new FileSource(uriString));
                    pdfViewer.Sayfalar = new ObservableCollection<int>(Enumerable.Range(1, pdfViewer.Mpp.TotalPages));
                    pdfViewer.ŞuankiSayfa = 1;
                    pdfViewer.ToplamSayfa = (int)(pdfViewer.Mpp?.TotalPages);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "EBYS", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void BtnRotateLeft_Click(object sender, RoutedEventArgs e) => Mpp?.RotateLeft();

        private void BtnRotateRight_Click(object sender, RoutedEventArgs e) => Mpp?.RotateRight();

        private void Mpp_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                {
                    return;
                }

                if (Mpp?.CurrentSource != null)
                {
                    ControlsActive = true;
                    Mpp?.ZoomToWidth();
                    CustomZoomLevel = Mpp.CurrentZoom;
                    ToplamSayfa = Mpp.TotalPages;
                    Sayfalar = new ObservableCollection<int>(Enumerable.Range(1, ToplamSayfa));
                    ŞuankiSayfa = 1;
                }
                else
                {
                    ControlsActive = false;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "EBYS", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void Mpp_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Mpp?.CurrentSource != null)
            {
                ŞuankiSayfa = Mpp.GetCurrentPageNumber();
            }
        }

        private void Mpp_Scroll(object sender, ScrollEventArgs e) => ŞuankiSayfa = Mpp.GetCurrentPageNumber();

        private void PdfViewerBack_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = ŞuankiSayfa > 1;

        private void PdfViewerBack_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Mpp?.GotoPreviousPage();
            ŞuankiSayfa = Mpp.PageRowDisplay == MoonPdfLib.PageRowDisplayType.ContinuousPageRows
                ? Mpp.GetCurrentPageNumber() + 1
                : Mpp.GetCurrentPageNumber();
        }

        private void PdfViewerNext_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = ŞuankiSayfa < Mpp.TotalPages;

        private void PdfViewerNext_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Mpp?.GotoNextPage();
            ŞuankiSayfa = Mpp.PageRowDisplay == MoonPdfLib.PageRowDisplayType.ContinuousPageRows
                ? Mpp.GetCurrentPageNumber() + 1
                : Mpp.GetCurrentPageNumber();
        }

        private void PdfViewerOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;

        private void PdfViewerOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Pdf Dosyası |*.pdf", Multiselect = false };
            if (openFileDialog.ShowDialog() == true)
            {
                Mpp?.Open(new FileSource(openFileDialog.FileName));
                ToplamSayfa = Mpp.TotalPages;
                Sayfalar = new ObservableCollection<int>(Enumerable.Range(1, ToplamSayfa));
                ŞuankiSayfa = 1;
            }
        }

        private void PdfViewerPrint_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Mpp?.CurrentSource != null;

        private void PdfViewerPrint_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog
            {
                PageRangeSelection = PageRangeSelection.AllPages,
                UserPageRangeEnabled = true,
                MaxPage = (uint)Mpp?.TotalPages,
                MinPage = 1
            };
            if (pd.ShowDialog() == true)
            {
                DrawingVisual dv = new DrawingVisual();
                int başlangıç;
                int bitiş;
                if (pd.PageRangeSelection == PageRangeSelection.AllPages)
                {
                    başlangıç = 1;
                    bitiş = Mpp.TotalPages;
                }
                else
                {
                    başlangıç = pd.PageRange.PageFrom;
                    bitiş = pd.PageRange.PageTo;
                }

                for (int i = başlangıç; i <= bitiş; i++)
                {
                    using (Bitmap bmp = MuPdfWrapper.ExtractPage(Mpp?.CurrentSource, i, 4))
                    {
                        using (DrawingContext dc = dv.RenderOpen())
                        {
                            BitmapSource bitmapSource = bmp.Width > bmp.Height ? bmp.ToBitmapImage(ImageFormat.Jpeg).Resize((int)pd.PrintableAreaHeight, (int)pd.PrintableAreaWidth, 90, 300, 300) : bmp.ToBitmapImage(ImageFormat.Jpeg).Resize((int)pd.PrintableAreaWidth, (int)pd.PrintableAreaHeight, 0, 300, 300);
                            bitmapSource.Freeze();
                            dc.DrawImage(bitmapSource, new Rect(0, 0, pd.PrintableAreaWidth, pd.PrintableAreaHeight));
                        }
                    }

                    pd.PrintVisual(dv, "");
                }
            }
        }

        private void PdfViewerSave_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Mpp?.CurrentSource != null;

        private void PdfViewerSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "Jpg Dosyası (*.jpg)|*.jpg" };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllBytes(saveFileDialog.FileName, PdfExtractSmallPreviewImage(this,ŞuankiSayfa, 2).ToTiffJpegByteArray(Extensions.ExtensionMethods.Format.Jpg));
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            if (Mpp?.CurrentSource != null)
            {
                Mpp?.Zoom(CustomZoomLevel);
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            LbSayfalar.Visibility = Visibility.Visible;
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            LbSayfalar.Visibility = Visibility.Collapsed;
        }

        private void UniformGridPrint_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Mpp?.CurrentSource != null;

        private void UniformGridPrint_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            DrawingVisual dv = new DrawingVisual();
            string btniçerik = (e.OriginalSource as Button)?.Content as string;
            for (int i = 1; i <= Convert.ToInt32(btniçerik); i++)
            {
                using (Bitmap bmp = MuPdfWrapper.ExtractPage(Mpp?.CurrentSource, Mpp.GetCurrentPageNumber(), 4))
                {
                    using (DrawingContext dc = dv.RenderOpen())
                    {
                        BitmapSource bitmapSource = bmp.Width > bmp.Height ? bmp.ToBitmapImage(ImageFormat.Jpeg).Resize((int)pd.PrintableAreaHeight, (int)pd.PrintableAreaWidth, 90, 300, 300) : bmp.ToBitmapImage(ImageFormat.Jpeg).Resize((int)pd.PrintableAreaWidth, (int)pd.PrintableAreaHeight, 0, 300, 300);
                        bitmapSource.Freeze();
                        dc.DrawImage(bitmapSource, new Rect(0, 0, pd.PrintableAreaWidth, pd.PrintableAreaHeight));
                    }
                }

                pd.PrintVisual(dv, "");
            }
        }

        private void ZoomDown_Click(object sender, RoutedEventArgs e)
        {
            Mpp?.ZoomOut();
            CustomZoomLevel = Mpp.CurrentZoom;
        }

        private void ZoomHeight_Click(object sender, RoutedEventArgs e)
        {
            Mpp?.ZoomToHeight();
            CustomZoomLevel = Mpp.CurrentZoom;
        }

        private void ZoomUp_Click(object sender, RoutedEventArgs e)
        {
            Mpp?.ZoomIn();
            CustomZoomLevel = Mpp.CurrentZoom;
        }

        private void ZoomWidth_Click(object sender, RoutedEventArgs e)
        {
            Mpp?.ZoomToWidth();
            CustomZoomLevel = Mpp.CurrentZoom;
        }
    }

    public class NullableToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => parameter != null ? value == null : value != null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class PdfSayfaSayıToBitmapConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return !(values[0] is MoonPdfViewer moonPdfViewer) || !int.TryParse(values[1].ToString(), out int sayfano)
                ? DependencyProperty.UnsetValue
                :MoonPdfViewer.PdfExtractSmallPreviewImage(moonPdfViewer,sayfano);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
using Extensions;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{
    public partial class DataEnterWindow : UserControl, IDisposable
    {
        private bool disposedValue;

        public DataEnterWindow() => InitializeComponent();

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && DataContext is Data data)
                {
                    data.ResimData = null;
                    data.PdfData = null;
                    data.Dosyalar = null;
                    data.ResimYolu = null;
                }

                disposedValue = true;
            }
        }

        private void CameraUserControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (DataContext is Data data && e.PropertyName == "ResimData")
            {
                data.ResimData = ((sender as Extensions.CameraUserControl)?.ResimData).WebpEncode(data.WebpQuality);
                data.DosyaUzantı = ".webp";
                data.Boyut = data.ResimData.Length / 1024;
            }
        }

        private void TwainCtrl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (DataContext is Data data)
            {
                if (e.PropertyName == "SeçiliResim")
                {
                    data.ResimData = ((sender as TwainControl.TwainCtrl)?.SeçiliResim).ToTiffJpegByteArray(ExtensionMethods.Format.Jpg).WebpEncode(data.WebpQuality);
                    data.DosyaUzantı = ".webp";
                    data.Boyut = data.ResimData.Length / 1024;
                }

                if (e.PropertyName == "SeçiliResimler")
                {
                    IList SeçiliResimler = (sender as TwainControl.TwainCtrl)?.SeçiliResimler;
                    using MemoryStream ms = new();
                    SeçiliResimler.CreatePdfFile().Save(ms);
                    data.PdfData = ms.ToArray();
                    data.DosyaUzantı = ".pdf";
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) => (TryFindResource("FilteredCvs") as CollectionViewSource).Filter += (s, e) => e.Accepted = DateTime.Parse((e.Item as XmlNode)?["Gun"]?.InnerText) == (DataContext as Data)?.TamTarih;

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) => Dispose(true);
    }
}
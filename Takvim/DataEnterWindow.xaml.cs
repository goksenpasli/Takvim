using System;
using System.ComponentModel;
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
                    data.Dosyalar = null;
                }

                disposedValue = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) => (TryFindResource("FilteredCvs") as CollectionViewSource).Filter += (s, e) => e.Accepted = DateTime.Parse((e.Item as XmlNode)?["Gun"]?.InnerText) == (DataContext as Data)?.TamTarih;

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) => Dispose(true);

        private void TwainCtrl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SeçiliResim" && DataContext is Data data)
            {
                data.ResimData = ((sender as TwainControl.TwainCtrl)?.SeçiliResim).ToTiffJpegByteArray(ExtensionMethods.Format.Jpg).WebpEncode(data.WebpQuality);
                data.ResimUzantı = ".webp";
                data.Boyut = data.ResimData.Length / 1024;
            }
        }
    }
}
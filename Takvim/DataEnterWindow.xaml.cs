using System;
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

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e) => (TryFindResource("FilteredCvs") as CollectionViewSource).Filter += (s, e) => e.Accepted = DateTime.Parse((e.Item as XmlNode)?["Gun"]?.InnerText) == (DataContext as Data)?.TamTarih;
        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e) => Dispose(true);
    }
}
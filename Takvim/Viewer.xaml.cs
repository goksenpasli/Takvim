using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Takvim
{
    public partial class Viewer : Window, IDisposable
    {
        private bool disposedValue;

        public Viewer()
        {
            InitializeComponent();
            DataContext = new ImageViewer();
        }

        public Viewer(XmlElement xmldata) : this()
        {
            Img.Source = (BitmapSource)new Base64ImageConverter().Convert(xmldata["Resim"].InnerText, null, null, CultureInfo.CurrentCulture);
            Tb.DataContext = xmldata;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Img.Source = null;
                    Tb.DataContext = null;
                }

                disposedValue = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Dispose(true);
    }
}

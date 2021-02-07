using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public Viewer(string file) : this() => Img.Source = (BitmapSource)new Base64ImageConverter().Convert(file, null, null, CultureInfo.CurrentCulture);

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
                }

                disposedValue = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Dispose(true);
    }
}

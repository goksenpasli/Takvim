using System;
using System.Windows;
using System.Xml;

namespace Takvim
{
    public partial class Viewer : Window, IDisposable
    {
        private bool disposedValue;

        public Viewer() => InitializeComponent();

        public Viewer(XmlElement xmldata) : this() => DataContext = new ImageViewer(xmldata);

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
                    //DataContext = null;
                }

                disposedValue = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => Dispose(true);
    }
}
﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Takvim
{
    public class ImageViewer : InpcBase
    {
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

    public partial class Viewer : Window, IDisposable
    {
        private bool disposedValue;

        public Viewer()
        {
            InitializeComponent();
            DataContext = new ImageViewer();
        }

        public Viewer(string file) : this() => Img.Source = (ImageSource)new Base64ImageConverter().Convert(file, null, null, CultureInfo.CurrentCulture);

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

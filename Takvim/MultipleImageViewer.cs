using System;
using System.Collections.ObjectModel;
using System.Drawing;
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
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Windows;

namespace Takvim
{
    public class MultipleImageViewer:InpcBase
    {

        private ObservableCollection<MultipleImage> topluDosyalar;

        public MultipleImageViewer()
        {
            TopluDosyalar = new ObservableCollection<MultipleImage>();

            if (MainViewModel.xmlDataProvider?.Document is XmlDocument xmlDocument)
            {
                foreach (XmlNode item in xmlDocument.SelectNodes("/Veriler/Veri"))
                {
                    if (item["Resim"]?.InnerText is not null)
                    {
                        TopluDosyalar.Add(new MultipleImage() { Resim = item["Resim"].InnerText });
                    }
                }
            }
        }

        public ObservableCollection<MultipleImage> TopluDosyalar
        {
            get => topluDosyalar;

            set
            {
                if (topluDosyalar != value)
                {
                    topluDosyalar = value;
                    OnPropertyChanged(nameof(TopluDosyalar));
                }
            }
        }


    }
}
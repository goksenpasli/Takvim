using Extensions;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Xml;

namespace Takvim
{
    public class MultipleImageViewer : InpcBase
    {
        private ObservableCollection<MultipleImage> topluDosyalar;

        public MultipleImageViewer()
        {
            TopluDosyalar = new ObservableCollection<MultipleImage>();
            BrushConverter brushConverter = new();
            if (MainViewModel.xmlDataProvider?.Document is XmlDocument xmlDocument)
            {
                foreach (XmlNode item in xmlDocument.SelectNodes("/Veriler/Veri"))
                {
                    if (item["Resim"]?.InnerText is not null)
                    {
                        TopluDosyalar.Add(new MultipleImage()
                        {
                            VeriRenk = (Brush)brushConverter.ConvertFromString(item.Attributes.GetNamedItem("Renk").InnerText) ?? Brushes.Transparent,
                            Resim = item["Resim"].InnerText,
                            GünNotAçıklama = item["Aciklama"].InnerText
                        });
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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{
    public class AyTekrarDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Data data && MainViewModel.xmlDataProvider?.Data is ICollection<XmlNode> xmlNode)
            {
                List<Data> TekrarGünlerVerileri = new();

                foreach (XmlNode item in xmlNode.Where(z => z.Attributes["AyTekrar"]?.InnerText == "true"))
                {
                    int.TryParse(item.Attributes["TekrarGun"]?.InnerText, out int tekrargün);
                    if (data.TamTarih.Day == tekrargün && data.TamTarih > DateTime.Today)
                    {
                        Data veri = new()
                        {
                            GünNotAçıklama = item["Aciklama"]?.InnerText,
                            TamTarih = DateTime.Parse(item["Gun"]?.InnerText)
                        };
                        if (item["Resim"]?.InnerText != null)
                        {
                            veri.ResimData = System.Convert.FromBase64String(item["Resim"]?.InnerText);
                        }
                        TekrarGünlerVerileri.Add(veri);
                    }
                }
                return TekrarGünlerVerileri;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
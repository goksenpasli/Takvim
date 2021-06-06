using System;
using System.Net;
using System.Windows.Controls;
using System.Windows.Data;

namespace Takvim
{
    public partial class PrayerTimeControl : UserControl
    {
        public PrayerTimeControl()
        {
            InitializeComponent();
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            DataContext = new Prayer((XmlDataProvider)TryFindResource("Data"));
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (e.Item as Prayer)?.Tarih == DateTime.Today;
        }
    }
}
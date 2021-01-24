using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;

namespace Takvim
{
    /// <summary>
    /// Interaction logic for DataEnterWindow.xaml
    /// </summary>
    public partial class DataEnterWindow : UserControl
    {
        public DataEnterWindow()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e) => (TryFindResource("FilteredCvs") as CollectionViewSource).Filter += (s, e) => e.Accepted = DateTime.Parse((e.Item as XmlNode)?["Gun"].InnerText) == (DataContext as Data)?.TamTarih;
    }
}
using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml;

namespace Takvim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void CollectionViewSource_Filter(object sender, System.Windows.Data.FilterEventArgs e) => e.Accepted = DateTime.Parse((e.Item as XmlNode)?["Gun"].InnerText) == DateTime.Today;
    }
}
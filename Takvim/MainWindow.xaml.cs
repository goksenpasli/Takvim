using System;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if ((DataContext as MainViewModel)?.FilteredCvs is not null)
            {
                (DataContext as MainViewModel).FilteredCvs.Filter += (s, e) => e.Accepted = DateTime.Parse((e.Item as XmlNode)?["Gun"]?.InnerText) == DateTime.Today;
            }
        }
    }
}
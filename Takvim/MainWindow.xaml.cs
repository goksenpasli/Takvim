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

        private void CollectionViewSource_Filter(object sender, System.Windows.Data.FilterEventArgs e) => e.Accepted = DateTime.Parse((e.Item as XmlNode)?["Gun"].InnerText) == DateTime.Today;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainViewModel.AppNotifyIcon.Dispose();
            MainViewModel.AppNotifyIcon = null;
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (MainViewModel.AppNotifyIcon != null)
            {
                MainViewModel.AppNotifyIcon.Visible = !IsVisible;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
            else
            {
                MainViewModel.AppWindowState = WindowState;
            }
        }
    }
}
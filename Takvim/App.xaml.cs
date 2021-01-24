using System.Windows;

namespace Takvim
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Closing += MainWindow_Closing;
            mainWindow.IsVisibleChanged += MainWindow_IsVisibleChanged;
            mainWindow.StateChanged += MainWindow_StateChanged;
            if (e.Args.Length > 0 && e.Args[0] == "/MINIMIZE")
            {
                MainViewModel.AppNotifyIcon.Visible = true;
                mainWindow.Hide();
            }
            else
            {
                mainWindow.Show();
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainViewModel.AppNotifyIcon.Dispose();
            MainViewModel.AppNotifyIcon = null;
        }

        private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (MainViewModel.AppNotifyIcon != null)
            {
                MainViewModel.AppNotifyIcon.Visible = (sender as MainWindow)?.IsVisible == false;
            }
        }

        private void MainWindow_StateChanged(object sender, System.EventArgs e)
        {
            MainWindow mainWindow = sender as MainWindow;
            if (mainWindow.WindowState == WindowState.Minimized)
            {
                mainWindow.Hide();
            }
            else
            {
                MainViewModel.AppWindowState = mainWindow.WindowState;
            }
        }
    }
}
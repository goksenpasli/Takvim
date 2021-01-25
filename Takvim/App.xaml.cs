using System.Threading;
using System.Windows;

namespace Takvim
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string AppId = "7cf5ff9d-1479-436b-a0a2-954d4a72190a";

        private readonly Semaphore instancesAllowed = new Semaphore(1, 1, AppId);

        private bool WasRunning {get;}

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (WasRunning)
            {
                instancesAllowed.Release();
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (instancesAllowed.WaitOne(1000))
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
                return;
            }
            MessageBox.Show("Uygulama Zaten Çalışıyor.", "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            Shutdown();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainViewModel.AppNotifyIcon.Dispose();
            MainViewModel.AppNotifyIcon = null;
            MainViewModel.timer?.Stop();
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
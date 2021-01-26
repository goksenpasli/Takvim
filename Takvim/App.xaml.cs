using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Takvim
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string AppId = "7cf5ff9d-1479-436b-a0a2-954d4a72190a";

        private readonly Semaphore instancesAllowed = new Semaphore(1, 1, AppId);

        private bool WasRunning { get; }

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
                mainWindow.Loaded += MainWindow_Loaded;
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
            MainViewModel.duyurularwindow?.Close();
        }

        private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (MainViewModel.AppNotifyIcon != null)
            {
                MainViewModel.AppNotifyIcon.Visible = (sender as MainWindow)?.IsVisible == false;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 5, 0)
            };
            timer.Start();
            timer.Tick += (s, e) =>
            {
                MainWindow mainWindow = sender as MainWindow;
                (mainWindow.DataContext as MainViewModel)?.DuyurularPopupEkranıAç.Execute(null);
                DispatcherTimer visibilitytimer = new DispatcherTimer
                {
                    Interval = new TimeSpan(0, 0, 10)
                };
                visibilitytimer.Start();
                visibilitytimer.Tick += (s, e) =>
                {
                    MainViewModel.duyurularwindow?.Close();
                    visibilitytimer.Stop();
                };
            };
        }
        private void MainWindow_StateChanged(object sender, EventArgs e)
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
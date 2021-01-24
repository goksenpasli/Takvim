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
            if (e.Args.Length>0 && e.Args[0] == "/MINIMIZE")
            {
                MainViewModel.AppNotifyIcon.Visible = true;
                mainWindow.Hide();
            }
            else
            {
                mainWindow.Show();
            }
        }
    }
}
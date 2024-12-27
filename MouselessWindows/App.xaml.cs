using System.Configuration;
using System.Data;
using System.Windows;

namespace MouselessWindows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = new MainWindow();
            MainWindow.Show();
            MainWindow.Hide();

            GridOverlay gridOverlay = new GridOverlay();
            gridOverlay.Show();

            //MainWindow.Show();
            //MainWindow.Hide();
            //MainWindow.Closing += MainWindow_Closing;

            //_notifyIcon = new System.Windows.Forms.NotifyIcon();
            //_notifyIcon.DoubleClick += (s, args) => ShowMainWindow();
            //_notifyIcon.Icon = new System.Drawing.Icon("Assets/AppIcon.ico");
            //_notifyIcon.Visible = true;

            //CreateContextMenu();

            //RegisterGlobalHotKey();
        }
    }

}

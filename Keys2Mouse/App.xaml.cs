using System.Configuration;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;
using System;
using System.ComponentModel;
using System.Windows.Interop;
using System.Diagnostics;

namespace Keys2Mouse
{
    public partial class App : System.Windows.Application
    {
        private System.Windows.Forms.NotifyIcon? _notifyIcon;
        private bool _isExit;
        private const int HOTKEY_ID = 9000;
        private const uint MOD_WIN = 0x0008;
        private const uint MOD_CONTROL = 0x0002;
        private const uint VK_CONTROL = 0x11;
        GridOverlay gridOverlay = null;
        SettingsWindow settings = null;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = new MainWindow();
            MainWindow.Show();
            MainWindow.Hide();

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.DoubleClick += (s, args) => ShowSettings();
            _notifyIcon.Icon = new System.Drawing.Icon("Assets/icon.ico");
            _notifyIcon.Visible = true;
            CreateContextMenu();

            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(MainWindow).Handle;
            RegisterHotKey(hwnd, HOTKEY_ID, MOD_WIN | MOD_CONTROL, VK_CONTROL);

            var source = System.Windows.Interop.HwndSource.FromHwnd(hwnd);
            source.AddHook(HwndHook);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                ShowGridOverlay();
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void ShowGridOverlay()
        {
            if (gridOverlay == null)
            {
                gridOverlay = new GridOverlay();
                gridOverlay.Closed += (s, e) => gridOverlay = null;
                gridOverlay.Show();
            }
            else
            {
                gridOverlay.Close();
                gridOverlay = null;
            }
        }


        protected override void OnExit(ExitEventArgs e)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(MainWindow).Handle;
            UnregisterHotKey(hwnd, HOTKEY_ID);
            base.OnExit(e);
        }

        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Settings").Click += (s, e) => ShowSettings();
            _notifyIcon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => ExitApplication();
        }

        private void ExitApplication()
        {
            _isExit = true;
            MainWindow.Close();
        }

        public void ShowSettings()
        {
            if (settings == null)
            {
                settings = new SettingsWindow();
                settings.Closed += (s, e) => settings = null;
                settings.Show();
            }
        }
    }
}

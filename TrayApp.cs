using System;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace ScreenRecorderTray
{
    public class TrayApp : IDisposable
    {
        private readonly WinForms.NotifyIcon _trayIcon;
        private readonly HotkeyWindow _messageWindow;
        private readonly RecorderController _controller;

        private const int HOTKEY_ID = 1;

        public TrayApp()
        {
            _controller = new RecorderController();

            _messageWindow = new HotkeyWindow();
            _messageWindow.HotkeyPressed += OnHotkeyPressed;

            RegisterGlobalHotkey();

            _trayIcon = new WinForms.NotifyIcon
            {
                Visible = true,
                Text = "Screen Recorder",
                Icon = System.Drawing.SystemIcons.Application,
                ContextMenuStrip = BuildContextMenu()
            };
        }

        private WinForms.ContextMenuStrip BuildContextMenu()
        {
            var menu = new WinForms.ContextMenuStrip();
            menu.Items.Add("Quit", null, (_, __) =>
            {
                Application.Current.Shutdown();
            });
            return menu;
        }

        private void RegisterGlobalHotkey()
        {
            // Ctrl + Shift + R
            const uint MOD_CONTROL = 0x2;
            const uint MOD_SHIFT = 0x4;
            const uint VK_R = 0x52;

            if (!HotkeyWindow.RegisterHotKey(_messageWindow.Handle, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, VK_R))
            {
                System.Diagnostics.Debug.WriteLine("RegisterHotKey failed");
            }
        }

        private void OnHotkeyPressed(object? sender, EventArgs e)
        {
            _controller.ToggleRecording();
        }

        public void Dispose()
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _messageWindow.Dispose();
        }
    }
}

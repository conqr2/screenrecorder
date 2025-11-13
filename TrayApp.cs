using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using WinForms = System.Windows.Forms;

namespace ScreenRecorderTray
{
    public class TrayApp : IDisposable
    {
        private readonly NotifyIcon _trayIcon;
        private readonly HotkeyWindow _messageWindow;
        private readonly RecorderController _controller;

        private const int HOTKEY_ID = 1;

        public TrayApp()
        {
            _controller = new RecorderController();

            _messageWindow = new HotkeyWindow();
            _messageWindow.HotkeyPressed += OnHotkeyPressed;

            RegisterGlobalHotkey();

            _trayIcon = new NotifyIcon
            {
                Visible = true,
                Text = "Screen Recorder",
                 Icon = System.Drawing.SystemIcons.Application,  // Comment out or remove this line
                ContextMenuStrip = BuildContextMenu()
            };
            /*
            var assembly = Assembly.GetExecutingAssembly();
          //Console.Write(assembly.GetManifestResourceNames());
            string resourceName = "ScreenRecorderTray.custom_icon.png";  // Replace with your actual embedded resource name (check via assembly.GetManifestResourceNames() if unsure)
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
                }
                using (var bmp = new Bitmap(stream))
                {
                    _trayIcon.Icon = Icon.FromHandle(bmp.GetHicon());
                }
            }*/
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Quit", null, (_, __) =>
            {
                System.Windows.Application.Current.Shutdown();
            });
            return menu;
        }

        private void RegisterGlobalHotkey()
        {
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
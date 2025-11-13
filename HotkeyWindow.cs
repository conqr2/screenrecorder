using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using WinForms = System.Windows.Forms;

namespace ScreenRecorderTray
{
    internal class HotkeyWindow : WinForms.NativeWindow, IDisposable
    {
        private const int WM_HOTKEY = 0x0312;

        public event EventHandler? HotkeyPressed;

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public HotkeyWindow()
        {
            CreateHandle(new WinForms.CreateParams());
        }

        protected override void WndProc(ref WinForms.Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            UnregisterHotKey(Handle, 1);
            DestroyHandle();
        }
    }
}

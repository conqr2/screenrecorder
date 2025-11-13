using System.Windows;

namespace ScreenRecorderTray
{
    public partial class App : Application
    {
        private TrayApp _trayApp;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _trayApp = new TrayApp();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayApp?.Dispose();
            base.OnExit(e);
        }
    }
}

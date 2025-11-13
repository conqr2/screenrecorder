using System.Windows;

namespace ScreenRecorderTray
{
    public static class RegionSelectionWindowHelpers
    {
        public static Rect SelectScreenRegion()
        {
            var vs = System.Windows.Forms.SystemInformation.VirtualScreen;

            var win = new RegionSelectionWindow
            {
                Left = vs.Left,
                Top = vs.Top,
                Width = vs.Width,
                Height = vs.Height
            };

            var result = win.ShowDialog();
            return result == true ? win.SelectedRegion : Rect.Empty;
        }
    }
}

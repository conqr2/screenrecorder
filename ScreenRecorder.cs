// ScreenRecorder.cs
using System.Diagnostics;
using System.Windows;

namespace ScreenRecorderTray
{
    public class ScreenRecorder
    {
        private readonly Rect _region;
        private Process? _ffmpeg;

        public ScreenRecorder(Rect region)
        {
            _region = region;
        }

        public void Start(string outputPath)
        {
            int x = (int)_region.X;
            int y = (int)_region.Y;
            int w = (int)_region.Width;
            int h = (int)_region.Height;

            string video = $"-f gdigrab -framerate 30 -offset_x {x} -offset_y {y} -video_size {w}x{h} -draw_mouse 1 -i desktop";
            string audio = "-f dshow -i audio=\"Microphone (Your Mic Name Here)\"";
            string outArgs = "-c:v libx264 -preset veryfast -c:a aac -pix_fmt yuv420p";

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"{video} {audio} {outArgs} \"{outputPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _ffmpeg = Process.Start(psi);
        }

        public void Stop()
        {
            if (_ffmpeg == null) return;
            try
            {
                if (!_ffmpeg.HasExited)
                {
                    _ffmpeg.Kill();
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}

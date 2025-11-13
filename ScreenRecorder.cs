// ScreenRecorder.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ScreenRecorderTray
{
    public class ScreenRecorder
    {
        private readonly Rect _region;
        private Process? _ffmpeg;

        // Look for ffmpeg.exe in the same directory as the app
        private static readonly string FfmpegPath =
            Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe");

        public ScreenRecorder(Rect region)
        {
            _region = region;
        }

        public void Start(string outputPath)
        {
            if (!File.Exists(FfmpegPath))
            {
                MessageBox.Show(
                    $"ffmpeg.exe not found next to the application.\n\nExpected at:\n{FfmpegPath}",
                    "ScreenRecorder",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            int x = (int)_region.X;
            int y = (int)_region.Y;
            int w = (int)_region.Width;
            int h = (int)_region.Height;

            string video =
                $"-f gdigrab -framerate 30 -offset_x {x} -offset_y {y} -video_size {w}x{h} -draw_mouse 1 -i desktop";

            // TODO: change to your real microphone device name.
            // For now you can even comment out audio entirely to test.
            //string audio = "-f dshow -i audio=\"Microphone (Your Mic Name Here)\"";
            string audio = "";

            string outArgs = "-c:v libx264 -preset veryfast -c:a aac -pix_fmt yuv420p";

            var psi = new ProcessStartInfo
            {
                FileName = FfmpegPath,
                Arguments = $"{video} {audio} {outArgs} \"{outputPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                _ffmpeg = Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to start ffmpeg:\n\n" + ex.Message,
                    "ScreenRecorder",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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

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

        private static readonly string FfmpegPath =
            Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe");

        public ScreenRecorder(Rect region)
        {
            _region = region;
        }

        public bool Start(string outputPath)
        {
            if (!File.Exists(FfmpegPath))
            {
                MessageBox.Show(
                    $"ffmpeg.exe not found next to the application.\n\nExpected at:\n{FfmpegPath}",
                    "ScreenRecorder",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            int x = (int)Math.Round(_region.X);
            int y = (int)Math.Round(_region.Y);
            int w = (int)Math.Round(_region.Width);
            int h = (int)Math.Round(_region.Height);

            // Make dimensions even for x264
            if ((w & 1) == 1) w--;
            if ((h & 1) == 1) h--;

            if (w <= 0 || h <= 0)
            {
                MessageBox.Show("Selected region is too small to record.",
                    "ScreenRecorder",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            // VIDEO ONLY for now (no audio)
            string video =
    $"-y -f gdigrab -framerate 30 -offset_x {x} -offset_y {y} -video_size {w}x{h} -draw_mouse 1 -i desktop";

            // Microphone capture (replace device name with your own)
            string audio =
                "-f dshow -i audio=\"Headset (USB-C to 3.5mm Headphone Jack Adapter)\"";

            // Encoding options: video + AAC audio
            string outArgs = "-c:v libx264 -preset veryfast -pix_fmt yuv420p -c:a aac -b:a 128k";

            string args = $"{video} {audio} {outArgs} \"{outputPath}\"";

            var psi = new ProcessStartInfo
            {
                FileName = FfmpegPath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardInput = true,   // so we can send 'q'
                RedirectStandardError = false,  // keep things simple
                CreateNoWindow = true
            };

            try
            {
                _ffmpeg = Process.Start(psi);
                return _ffmpeg != null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to start ffmpeg:\n\n" + ex,
                    "ScreenRecorder",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                _ffmpeg = null;
                return false;
            }
        }

        public void Stop()
        {
            if (_ffmpeg == null) return;

            try
            {
                if (!_ffmpeg.HasExited)
                {
                    // Ask ffmpeg to stop like in the console
                    try
                    {
                        _ffmpeg.StandardInput.WriteLine("q");
                        _ffmpeg.StandardInput.Flush();
                    }
                    catch
                    {
                        // stdin may already be closed, that's fine
                    }

                    // Wait indefinitely for it to finish and flush the file
                    _ffmpeg.WaitForExit();
                }
            }
            catch
            {
                // don't care for now; file is what matters
            }
            finally
            {
                _ffmpeg.Dispose();
                _ffmpeg = null;
            }
        }
    }
}

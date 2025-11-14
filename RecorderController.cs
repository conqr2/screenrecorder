// RecorderController.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NAudio.Wave;


namespace ScreenRecorderTray
{
    public class RecorderController
    {
        private enum State
        {
            Idle,
            Selecting,
            Recording
        }

        private State _state = State.Idle;
        private HelperOverlayWindow? _helper;
        private ScreenRecorder? _recorder;
        private DesktopAudioRecorder? _desktopRecorder;
        private string? _videoFilePath;
        private string? _desktopAudioPath;



        public void ToggleRecording()
        {
            switch (_state)
            {
                case State.Idle:
                    StartSelectionAndRecording();
                    break;
                case State.Recording:
                    StopRecording();
                    break;
            }
        }

        private void StartSelectionAndRecording()
        {
            _state = State.Selecting;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Rect region = RegionSelectionWindowHelpers.SelectScreenRegion();
                if (region.IsEmpty)
                {
                    _state = State.Idle;
                    return;
                }

                _helper = new HelperOverlayWindow();
                _helper.Show();

                // temp file paths
                string baseName = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                _videoFilePath = Path.Combine(Path.GetTempPath(), $"capture_{baseName}_video.mp4");
                _desktopAudioPath = Path.Combine(Path.GetTempPath(), $"capture_{baseName}_desktop.wav");

                // start video+mic via ffmpeg
                _recorder = new ScreenRecorder(region);
                bool started = _recorder.Start(_videoFilePath);
                if (!started)
                {
                    _helper.Close();
                    _helper = null;
                    _state = State.Idle;
                    return;
                }

                // start desktop audio via NAudio loopback
                _desktopRecorder = new DesktopAudioRecorder();
                _desktopRecorder.Start(_desktopAudioPath);

                _state = State.Recording;
            });
        }

        private bool MixDesktopAudioIntoVideo(string videoPath, string desktopWavPath, string outputPath)
        {
            try
            {
                // ffmpeg command:
                // ffmpeg -y -i videoPath -i desktopWavPath
                //   -filter_complex [0:a][1:a]amix=inputs=2:normalize=1[a]
                //   -map 0:v -map [a]
                //   -c:v copy -c:a aac -b:a 192k outputPath

                string ffmpegExe = Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe");
                if (!File.Exists(ffmpegExe))
                    return false;

                string args =
                    $"-y -i \"{videoPath}\" -i \"{desktopWavPath}\" " +
                    "-filter_complex [0:a][1:a]amix=inputs=2:normalize=1[a] " +
                    "-map 0:v -map [a] " +
                    "-c:v copy -c:a aac -b:a 192k " +
                    $"\"{outputPath}\"";

                var psi = new ProcessStartInfo
                {
                    FileName = ffmpegExe,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = false,
                    CreateNoWindow = true
                };

                using var proc = Process.Start(psi);
                if (proc == null) return false;

                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    string err = proc.StandardError.ReadToEnd();
                    System.Diagnostics.Debug.WriteLine("Mix ffmpeg error: " + err);
                    return false;
                }

                return File.Exists(outputPath) && new FileInfo(outputPath).Length > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MixDesktopAudioIntoVideo exception: " + ex);
                return false;
            }
        }

        private async void StopRecording()
        {
            if (_state != State.Recording) return;
            _state = State.Idle;

            // stop video recorder (ffmpeg)
            _recorder?.Stop();
            _recorder = null;

            // stop desktop audio recorder
            _desktopRecorder?.Stop();
            _desktopRecorder = null;

            if (_helper != null)
            {
                _helper.Dispatcher.Invoke(() => _helper.Close());
                _helper = null;
            }

            if (string.IsNullOrEmpty(_videoFilePath) || !File.Exists(_videoFilePath))
            {
                System.Diagnostics.Debug.WriteLine("No video file found after recording.");
                return;
            }

            // If no desktop audio file, just use video as-is
            string finalFilePath = _videoFilePath;
            if (!string.IsNullOrEmpty(_desktopAudioPath) && File.Exists(_desktopAudioPath))
            {
                // mix video’s (mic) audio + desktop wav into a new final file
                string mixedFile = Path.Combine(
                    Path.GetTempPath(),
                    Path.GetFileNameWithoutExtension(_videoFilePath) + "_mixed.mp4");

                bool mixed = await Task.Run(() => MixDesktopAudioIntoVideo(_videoFilePath!, _desktopAudioPath!, mixedFile));
                if (mixed)
                {
                    finalFilePath = mixedFile;
                    try
                    {
                        File.Delete(_videoFilePath!);
                        File.Delete(_desktopAudioPath!);
                    }
                    catch { }
                }
            }

            // upload finalFilePath and copy URL to clipboard
            string? url = null;
            try
            {
                url = await Uploader.UploadFileAsync(finalFilePath, CancellationToken.None);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Upload failed: " + ex);
            }

            if (!string.IsNullOrEmpty(url))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Clipboard.SetText(url);
                });
            }
        }
    }
}

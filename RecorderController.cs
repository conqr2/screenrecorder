// RecorderController.cs
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
        private string? _currentFilePath;

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

                _recorder = new ScreenRecorder(region);

                string tempFile = Path.Combine(
                    Path.GetTempPath(),
                    $"capture_{DateTime.UtcNow:yyyyMMdd_HHmmss}.mp4");

                _currentFilePath = tempFile;
                _recorder.Start(tempFile);

                _state = State.Recording;
            });
        }

        private async void StopRecording()
        {
            if (_state != State.Recording) return;
            _state = State.Idle;

            _recorder?.Stop();

            if (_helper != null)
            {
                _helper.Dispatcher.Invoke(() => _helper.Close());
                _helper = null;
            }

            if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath))
            {
                System.Diagnostics.Debug.WriteLine("No recording file to upload.");
                return;
            }
            /*
            string? url = null;

            try
            {
                // upload synchronously after ffmpeg is done
                url = await Uploader.UploadFileAsync(_currentFilePath, CancellationToken.None);
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
            }*/
        }
    }
}

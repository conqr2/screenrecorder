// RecorderController.cs
using System;
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
        private CancellationTokenSource? _uploadCts;
        private Task<string>? _uploadTask;

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

            // Must run on WPF dispatcher
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
                _uploadCts = new CancellationTokenSource();

                string tempFile = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    $"capture_{DateTime.UtcNow:yyyyMMdd_HHmmss}.mp4");

                _recorder.Start(tempFile);

                _uploadTask = Task.Run(() => Uploader.UploadFileAsync(tempFile, _uploadCts.Token));

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

            string? url = null;
            try
            {
                if (_uploadTask != null)
                    url = await _uploadTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Upload failed: " + ex.Message);
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

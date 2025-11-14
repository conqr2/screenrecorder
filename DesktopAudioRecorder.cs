using System;
using System.IO;
using NAudio.Wave;

namespace ScreenRecorderTray
{
    public class DesktopAudioRecorder : IDisposable
    {
        private WasapiLoopbackCapture? _capture;
        private WaveFileWriter? _writer;
        private string? _outputPath;

        public bool IsRecording => _capture != null;

        public void Start(string outputPath)
        {
            if (IsRecording) return;

            _outputPath = outputPath;

            _capture = new WasapiLoopbackCapture(); // default output device loopback
            _writer = new WaveFileWriter(outputPath, _capture.WaveFormat);

            _capture.DataAvailable += (s, e) =>
            {
                _writer?.Write(e.Buffer, 0, e.BytesRecorded);
            };

            _capture.RecordingStopped += (s, e) =>
            {
                _writer?.Dispose();
                _writer = null;

                _capture?.Dispose();
                _capture = null;
            };

            _capture.StartRecording();
        }

        public void Stop()
        {
            if (_capture == null) return;

            _capture.StopRecording();
            // RecordingStopped event will clean up writer + capture
        }

        public void Dispose()
        {
            try
            {
                _capture?.StopRecording();
            }
            catch { }

            _writer?.Dispose();
            _capture?.Dispose();
            _capture = null;
            _writer = null;
        }
    }
}

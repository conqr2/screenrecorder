// Uploader.cs
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenRecorderTray
{
    public static class Uploader
    {
        private static readonly HttpClient Client = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(30)
        };

        public static async Task<string> UploadFileAsync(string path, CancellationToken ct)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Capture file not found", path);

            using var form = new MultipartFormDataContent();
            using var file = new StreamContent(File.OpenRead(path));
            file.Headers.ContentType = MediaTypeHeaderValue.Parse("video/mp4");
            form.Add(file, "file", Path.GetFileName(path));

            // TODO: change this to your real endpoint
            using var resp = await Client.PostAsync("https://your.server/upload", form, ct);
            resp.EnsureSuccessStatusCode();

            string json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("url").GetString() ?? "";
        }
    }
}

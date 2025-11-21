using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinigameMenu
{
    public sealed class MiniServer : IDisposable
    {
        private readonly string rootFolder;
        private readonly TcpListener listener;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private int port;

        public MiniServer(string rootFolder)
        {
            this.rootFolder = rootFolder;
            listener = new TcpListener(IPAddress.Loopback, 0); // 0 = random free port
        }

        public int Start()
        {
            listener.Start();
            port = ((IPEndPoint)listener.LocalEndpoint).Port;
            Task.Run(() => AcceptLoopAsync(cts.Token));
            return port;
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync(token);
                    _ = Task.Run(() => HandleClientAsync(client, token), token);
                }
            }
            catch (OperationCanceledException) { }
            catch { }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            using (client)
            using (var stream = client.GetStream())
            using (var reader = new StreamReader(stream, Encoding.ASCII, false, 8192, true))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false), 8192, true) { NewLine = "\r\n", AutoFlush = true })
            {
                // Read request line
                string? requestLine = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(requestLine)) return;

                // Basic: "GET /path HTTP/1.1"
                string[] parts = requestLine.Split(' ');
                if (parts.Length < 2) return;

                string path = parts[1];
                // Consume and ignore headers
                string? line;
                while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync())) { }

                // Default to index.html
                if (path == "/") path = "/index.html";

                // Normalize path and prevent directory traversal
                path = path.Replace('/', Path.DirectorySeparatorChar);
                path = path.TrimStart(Path.DirectorySeparatorChar);
                string fullPath = Path.GetFullPath(Path.Combine(rootFolder, path));
                string rootFull = Path.GetFullPath(rootFolder);
                if (!fullPath.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                {
                    await WriteNotFoundAsync(writer);
                    return;
                }

                if (!File.Exists(fullPath))
                {
                    await WriteNotFoundAsync(writer);
                    return;
                }

                // Write response
                byte[] bytes = await File.ReadAllBytesAsync(fullPath, token);
                string mime = GetMimeType(Path.GetExtension(fullPath));

                await writer.WriteLineAsync("HTTP/1.1 200 OK");
                await writer.WriteLineAsync($"Content-Type: {mime}");
                await writer.WriteLineAsync($"Content-Length: {bytes.Length}");
                await writer.WriteLineAsync("Connection: close");
                await writer.WriteLineAsync(""); // end headers
                await writer.FlushAsync();

                await stream.WriteAsync(bytes, 0, bytes.Length, token);
            }
        }

        private static async Task WriteNotFoundAsync(StreamWriter writer)
        {
            string body = "<h1>404 Not Found</h1>";
            await writer.WriteLineAsync("HTTP/1.1 404 Not Found");
            await writer.WriteLineAsync("Content-Type: text/html");
            await writer.WriteLineAsync($"Content-Length: {Encoding.UTF8.GetByteCount(body)}");
            await writer.WriteLineAsync("Connection: close");
            await writer.WriteLineAsync("");
            await writer.WriteAsync(body);
            await writer.FlushAsync();
        }

        private static string GetMimeType(string ext)
        {
            switch (ext.ToLowerInvariant())
            {
                case ".html": return "text/html";
                case ".htm":  return "text/html";
                case ".js":   return "application/javascript";
                case ".css":  return "text/css";
                case ".png":  return "image/png";
                case ".jpg":  return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                case ".gif":  return "image/gif";
                case ".svg":  return "image/svg+xml";
                case ".webp": return "image/webp";
                case ".mp3":  return "audio/mpeg";
                case ".wav":  return "audio/wav";
                case ".json": return "application/json";
                case ".woff": return "font/woff";
                case ".woff2":return "font/woff2";
                default:      return "application/octet-stream";
            }
        }

        public void Dispose()
        {
            try
            {
                cts.Cancel();
                listener.Stop();
            }
            catch { }
            finally
            {
                cts.Dispose();
            }
        }
    }
}

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading.Tasks;

//namespace ClientApp
//{
//    public class CustomHttpListener
//    {
//        private readonly List<string> prefixes = new List<string>();
//        private Socket listenerSocket;

//        public void Prefixes_Add(string prefix)
//        {
//            if (!prefix.EndsWith("/"))
//            {
//                prefix += "/";
//            }
//            prefixes.Add(prefix);
//        }

//        public void Start()
//        {
//            if (prefixes.Count == 0)
//            {
//                throw new InvalidOperationException("No prefixes added. Use Prefixes.Add() to specify listening URLs.");
//            }
//            Uri uri = new Uri(prefixes[0]);

//            // Resolve hostname to an IP address
//            IPAddress ipAddress;
//            if (!IPAddress.TryParse(uri.Host, out ipAddress))
//            {
//                // If not an IP address, resolve hostname to IP addresses
//                var addresses = Dns.GetHostAddresses(uri.Host);
//                if (addresses.Length == 0)
//                {
//                    throw new InvalidOperationException($"Unable to resolve host: {uri.Host}");
//                }
//                ipAddress = addresses[0]; // Use the first resolved IP address
//            }

//            // Determine the appropriate AddressFamily
//            AddressFamily addressFamily = ipAddress.AddressFamily;

//            // Create the listener socket with the appropriate address family
//            listenerSocket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);

//            // Bind the socket to the endpoint
//            IPEndPoint endPoint = new IPEndPoint(ipAddress, uri.Port);
//            listenerSocket.Bind(endPoint);
//            listenerSocket.Listen(100);

//            Console.WriteLine($"Listening on {uri}");

//        }

//        public async Task<HttpListenerContext> GetContextAsync()
//        {
//            if (listenerSocket == null)
//            {
//                throw new InvalidOperationException("Listener not started. Call Start() first.");
//            }

//            var clientSocket = await AcceptSocketAsync(listenerSocket);
//            return await CreateHttpContext(clientSocket);
//        }

//        private static Task<Socket> AcceptSocketAsync(Socket socket)
//        {
//            return Task.Factory.FromAsync(socket.BeginAccept, socket.EndAccept, null);
//        }
//        private async Task<HttpListenerContext> CreateHttpContext(Socket clientSocket)
//        {
//            var networkStream = new NetworkStream(clientSocket, ownsSocket: false); // Prevent auto-disposal of the socket
//            var buffer = new byte[4096];
//            int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);

//            if (bytesRead <= 0)
//            {
//                throw new InvalidOperationException("Failed to read request.");
//            }

//            // Parse HTTP request
//            string requestString = Encoding.UTF8.GetString(buffer, 0, bytesRead);
//            var requestLines = requestString.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
//            var requestLine = requestLines[0].Split(" ", StringSplitOptions.RemoveEmptyEntries);

//            if (requestLine.Length < 3)
//            {
//                throw new InvalidOperationException("Invalid HTTP request.");
//            }

//            string method = requestLine[0];
//            string url = requestLine[1];
//            string protocol = requestLine[2];

//            // Create HttpListenerRequest
//            var request = new HttpListenerRequest()
//            {
//                HttpMethod = method,
//                RawUrl = url,
//                ProtocolVersion = new Version(protocol.Replace("HTTP/", "")),
//                InputStream = networkStream,  // Set the InputStream
//                ContentEncoding = Encoding.UTF8 // Default to UTF-8 encoding
//            };

//            // Create HttpListenerResponse
//            HttpListenerResponse response = new HttpListenerResponse(networkStream);

//            // Return context
//            return new HttpListenerContext()
//            {
//                Request = request,
//                Response = response
//            };
//        }




//        public void Stop()
//        {
//            listenerSocket?.Close();
//            listenerSocket = null;
//        }
//    }
//    public class HttpListenerResponse
//    {
//        private readonly Stream responseStream;
//        private MemoryStream bufferStream = new MemoryStream();
//        private bool headersWritten = false; // Флаг для предотвращения повторной записи заголовков

//        public HttpListenerResponse(Stream stream)
//        {
//            responseStream = stream;
//            OutputStream = bufferStream; // Используем буфер для записи данных
//        }

//        public Stream OutputStream { get; }
//        public int StatusCode { get; set; }
//        public string ContentType { get; set; }
//        public long ContentLength64 { get; set; }

//        public async Task WriteAsync(byte[] buffer, int offset, int count)
//        {
//            await bufferStream.WriteAsync(buffer, offset, count);
//        }

//        public void Close()
//        {
//            if (!headersWritten)
//            {
//                WriteHeaders(); // Убедимся, что заголовки отправлены
//            }

//            try
//            {
//                if (bufferStream != null && bufferStream.CanRead)
//                {
//                    // Копируем содержимое буфера в основной поток
//                    bufferStream.Seek(0, SeekOrigin.Begin);
//                    bufferStream.CopyTo(responseStream);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Ошибка при записи данных в поток: {ex.Message}");
//            }
//            finally
//            {
//                try
//                {
//                    responseStream?.Flush(); // Проверяем, что поток доступен перед сбросом
//                }
//                catch (Exception flushEx)
//                {
//                    Console.WriteLine($"Ошибка при сбросе данных потока: {flushEx.Message}");
//                }

//                // Закрываем только те потоки, которые еще не были закрыты
//                if (responseStream != null && responseStream.CanWrite)
//                {
//                    responseStream.Close();
//                }

//                bufferStream?.Dispose(); // Безопасное освобождение ресурсов
//                bufferStream = null; // Сбрасываем ссылку, чтобы избежать повторного обращения
//            }
//        }


//private void WriteHeaders()
//{
//    if (headersWritten) return;

//    try
//    {
//        headersWritten = true;
//        ContentLength64 = bufferStream.Length; // Длина содержимого
//        string headers = $"HTTP/1.1 {StatusCode} OK\r\n" +
//                         $"Content-Type: {ContentType}\r\n" +
//                         $"Content-Length: {ContentLength64}\r\n" +
//                         "Connection: close\r\n" +
//                         "\r\n";
//        byte[] headerBytes = Encoding.UTF8.GetBytes(headers);
//        responseStream.Write(headerBytes, 0, headerBytes.Length);
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"Ошибка при записи заголовков: {ex.Message}");
//        throw; // Повторное выбрасывание исключения, чтобы знать об ошибке
//    }
//}

//    }




//    public class HttpListenerRequest
//    {
//        public string HttpMethod { get; set; }
//        public string RawUrl { get; set; }
//        public Version ProtocolVersion { get; set; }
//        public Stream InputStream { get; set; }
//        public Encoding ContentEncoding { get; set; }

//        // A dictionary to store query parameters
//        public Dictionary<string, string> QueryString { get; private set; } = new Dictionary<string, string>();

//        // Method to parse the query string from the RawUrl
//        public void ParseQueryString()
//        {
//            Uri url = new Uri($"http://localhost{RawUrl}"); // Creating a URI to parse the query string
//            var query = url.Query;

//            if (!string.IsNullOrEmpty(query))
//            {
//                var queryParams = query.TrimStart('?').Split('&');
//                foreach (var param in queryParams)
//                {
//                    var pair = param.Split('=');
//                    if (pair.Length == 2)
//                    {
//                        QueryString[pair[0]] = pair[1];
//                    }
//                }
//            }
//        }
//    }



//    public class HttpListenerContext
//    {
//        public HttpListenerRequest Request { get; set; }
//        public HttpListenerResponse Response { get; set; }
//    }
//}

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using ClientApp;
class WebSocketServer
{
    const int WebSocketPort = 8081;
    static List<User> registeredUsers = new List<User>();

    static async Task Main()
    {
        HttpListener httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://localhost:{WebSocketPort}/");
        httpListener.Start();
        Console.WriteLine($"WebSocket-сервер запущен на порту {WebSocketPort}");

        while (true)
        {
            try
            {
                var context = await httpListener.GetContextAsync();

                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    _ = HandleWebSocketConnection(wsContext.WebSocket);
                }
                else
                {
                    await HandleHttpRequest(context);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }

    static async Task HandleWebSocketConnection(WebSocket webSocket)
    {
        Console.WriteLine("Установлено WebSocket-соединение.");

        var buffer = new byte[8192];
        var jsonBuilder = new StringBuilder();
        WebSocketReceiveResult result;

        try
        {
            do
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);
                var receivedChunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                jsonBuilder.Append(receivedChunk);
            }
            while (!result.EndOfMessage);

            string json = jsonBuilder.ToString();
            Console.WriteLine($"Получен JSON: {json}");

            var data = JsonConvert.DeserializeObject<ImageData>(json);

            if (data != null)
            {
                var user = registeredUsers.FirstOrDefault(u => u.Username == data.Username);
                if (user != null)
                {
                    user.Gallery.Add(data);
                    Console.WriteLine("Изображение добавлено в галерею.");
                }
                else
                {
                    Console.WriteLine("Пользователь не найден.");
                }
            }
            else
            {
                Console.WriteLine("Десериализация вернула null. Проверьте формат JSON.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки WebSocket-соединения: {ex.Message}");
        }
        finally
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Соединение закрыто", System.Threading.CancellationToken.None);
        }
    }

    static async Task HandleHttpRequest(HttpListenerContext context)
    {
        try
        {
            var path = context.Request.RawUrl?.ToLower() ?? string.Empty;
            var method = context.Request.HttpMethod.ToUpper();

            if (method == "POST" && path == "/register")
            {
                await RegisterUser(context);
            }
            else if (method == "POST" && path.StartsWith("/likeimage"))
            {
                await LikeImage(context);
            }
            else if (method == "GET" && path.StartsWith("/getgallery"))
            {
                await GetUserGallery(context);
            }
            else if (method == "POST" && path.StartsWith("/inviteuser"))
            {
                await InviteUser(context);
            }
            else if (method == "GET" && path.StartsWith("/getinvitations"))
            {
                await GetInvitations(context);
            }
            else if (method == "GET" && path.StartsWith("/acceptinvitation"))
            {
                await AcceptInvitation(context);
            }
            else if (method == "GET" && path == "/styles.css")
            {
                await ServeStaticFile(context, "styles.css", "text/css");
            }
            else if (path == "/register.html" || path == "/view_images.html" || path == "/upload_image.html")
            {
                await ServeHtmlFile(context, path.TrimStart('/'));
            }
            else if (method == "POST" && path.StartsWith("/addcomment"))
            {
                await AddComment(context);
            }
            else if (method == "POST" && path == "/login")
            {
                await LoginUser(context);
            }

            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await context.Response.OutputStream.WriteAsync(new byte[0], 0, 0);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки HTTP-запроса: {ex.Message}");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.OutputStream.WriteAsync(new byte[0], 0, 0);
        }
        finally
        {
            context.Response.OutputStream.Close();
        }
    }

    static async Task RegisterUser(HttpListenerContext context)
    {
        using var reader = new StreamReader(context.Request.InputStream);
        var json = await reader.ReadToEndAsync();
        var userData = JsonConvert.DeserializeObject<User>(json);

        if (registeredUsers.Any(u => u.Username == userData.Username))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
        }
        else
        {
            registeredUsers.Add(userData);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }
    }
    static async Task LoginUser(HttpListenerContext context)
    {
        try
        {
            using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
            var json = await reader.ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<User>(json);

            if (data == null || string.IsNullOrWhiteSpace(data.Username))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            var user = registeredUsers.FirstOrDefault(u => u.Username == data.Username);

            if (user != null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка входа пользователя: {ex.Message}");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        finally
        {
            await context.Response.OutputStream.WriteAsync(new byte[0], 0, 0);
            context.Response.OutputStream.Close();
        }
    }

    static async Task GetUserGallery(HttpListenerContext context)
    {
        string username = context.Request.QueryString["username"];
        var user = registeredUsers.FirstOrDefault(u => u.Username == username);

        if (user != null)
        {
            var galleryJson = JsonConvert.SerializeObject(user.Gallery);
            byte[] buffer = Encoding.UTF8.GetBytes(galleryJson);
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
    }

    static async Task InviteUser(HttpListenerContext context)
    {
        string username = context.Request.QueryString["username"];
        string invitee = context.Request.QueryString["invitee"];

        var user = registeredUsers.FirstOrDefault(u => u.Username == username);
        var invitedUser = registeredUsers.FirstOrDefault(u => u.Username == invitee);

        if (user != null && invitedUser != null)
        {
            user.InvitedUsers.Add(invitee);
            invitedUser.Invitations.Add(username);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
    }

    static async Task GetInvitations(HttpListenerContext context)
    {
        string username = context.Request.QueryString["username"];
        var user = registeredUsers.FirstOrDefault(u => u.Username == username);

        if (user != null)
        {
            var invitationsJson = JsonConvert.SerializeObject(user.Invitations);
            byte[] buffer = Encoding.UTF8.GetBytes(invitationsJson);
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
    }

    static async Task AcceptInvitation(HttpListenerContext context)
    {
        string username = context.Request.QueryString["username"];
        string inviter = context.Request.QueryString["inviter"];

        var user = registeredUsers.FirstOrDefault(u => u.Username == username);
        var inviterUser = registeredUsers.FirstOrDefault(u => u.Username == inviter);

        if (user != null && inviterUser != null)
        {
            user.InvitedUsers.Add(inviter);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
    }

    static async Task ServeStaticFile(HttpListenerContext context, string filePath, string contentType)
    {
        if (File.Exists(filePath))
        {
            string content = await File.ReadAllTextAsync(filePath);
            byte[] buffer = Encoding.UTF8.GetBytes(content);

            context.Response.ContentType = contentType;
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
    }

    static async Task ServeHtmlFile(HttpListenerContext context, string filePath)
    {
        await ServeStaticFile(context, filePath, "text/html");
    }
    static async Task LikeImage(HttpListenerContext context)
    {
        try
        {
            // Считываем JSON из тела запроса
            using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();

            // Десериализация в словарь
            var likeData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if (!likeData.TryGetValue("Username", out string username) ||
                !likeData.TryGetValue("ImageName", out string imageName))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Invalid data format"));
                return;
            }

            // Поиск пользователя
            var user = registeredUsers.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("User not found"));
                return;
            }

            // Поиск изображения во всех галереях
            var image = registeredUsers
                .SelectMany(u => u.Gallery)
                .FirstOrDefault(img => img.Name == imageName);

            if (image == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Image not found"));
                return;
            }

            // Проверка на уже поставленный лайк
            if (image.LikedBy.Contains(username))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Already liked"));
                return;
            }

            // Добавляем лайк
            image.LikedBy.Add(username);
            image.Likes++;

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Like registered"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки лайка: {ex.Message}");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Internal server error"));
        }
        finally
        {
            context.Response.OutputStream.Close();
        }
    }
    static async Task AddComment(HttpListenerContext context)
    {
        try
        {
            using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
            string body = await reader.ReadToEndAsync();
            var commentRequest = JsonConvert.DeserializeObject<CommentRequest>(body);

            var user = registeredUsers.FirstOrDefault(u => u.Username == commentRequest.ImageOwner);
            if (user != null)
            {
                var image = user.Gallery.FirstOrDefault(img => img.Name == commentRequest.ImageName);
                if (image != null)
                {
                    var comment = new Comment
                    {
                        Username = commentRequest.Commenter,
                        Text = commentRequest.CommentText
                    };
                    image.Comments.Add(comment);

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    await context.Response.OutputStream.WriteAsync(new byte[0], 0, 0);
                    return;
                }
            }

            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await context.Response.OutputStream.WriteAsync(new byte[0], 0, 0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка добавления комментария: {ex.Message}");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.OutputStream.WriteAsync(new byte[0], 0, 0);
        }
        finally
        {
            context.Response.OutputStream.Close();
        }
    }




}




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
using ClientApp.Servecies;
using ClientApp.Models;
using System.Web;
class WebSocketServer
{
    const int WebSocketPort = 8081;
    static List<User> registeredUsers = new List<User>();
    const string UsersFilePath = "users.json";
    static JsonFileManager fileManager = new JsonFileManager(UsersFilePath, registeredUsers);
    static GalleriesService galleriesService;
    static ViewGalleryService viewGallaryService;
    static RegisterService registerService;
    static async Task Main()
    {
        registeredUsers = fileManager.LoadFromFile(); // Загрузка пользователей из файла при старте

        HttpListener httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://localhost:{WebSocketPort}/");
        httpListener.Start();
        Console.WriteLine($"WebSocket-сервер запущен на порту {WebSocketPort}");

        while (true)
        {
            try
            {
                var context = await httpListener.GetContextAsync();
                galleriesService = new GalleriesService(context, fileManager, registeredUsers);
                registerService = new RegisterService(context, fileManager, registeredUsers);
                viewGallaryService = new ViewGalleryService(context, fileManager, registeredUsers);
                await HandleHttpRequest(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
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
                await registerService.RegisterUser();
            }
            else if (method == "POST" && path.StartsWith("/likeimage"))
            {
                await viewGallaryService.LikeImage();
            }
            else if (path.StartsWith("/getgallery") && method == "GET")
            {
                await viewGallaryService.GetUserGallery();
            }
            else if (method == "POST" && path.StartsWith("/deleteimage"))
            {
                await galleriesService.DeleteImage();
            }
            else if (method == "GET" && path.StartsWith("/upload_image.html"))
            {
                await galleriesService.ServeUploadImagePage();
            }
            else if (method == "POST" && path.StartsWith("/inviteuser"))
            {
                await viewGallaryService.InviteUser();
            }
            else if (method == "GET" && path.StartsWith("/getinvitations"))
            {
                await viewGallaryService.GetInvitations();
            }
            else if (method == "GET" && path.StartsWith("/acceptinvitation"))
            {
                await viewGallaryService.AcceptInvitation();
            }
            else if (method == "GET" && path == "/styles.css")
            {
                await ServeStaticFile(context, "styles.css", "text/css");
            }
            else if (path == "/register.html" || path == "/view_images.html" || path == "/upload_image.html" || path == "/view_galleries.html")
            {
                await ServeHtmlFile(context, path.TrimStart('/'));
            }
            else if (path.StartsWith("/edit_image.html"))
            {
                await ServeHtmlFile(context, "edit_image.html");
            }
            else if (method == "POST" && path.StartsWith("/addcomment"))
            {
                await viewGallaryService.AddComment();
            }
            else if(method == "POST" && path.StartsWith("/getgalleries"))
            {
                await galleriesService.GetGalleries();
            }
            else if (method == "POST" && path == "/login")
            {
                await registerService.LoginUser();
            }
            else if (method == "POST" && path == "/addimage")
            {
                await galleriesService.AddImage();
            }
            else if (method == "POST" && path == "/creategallery")
            {
                await galleriesService.CreateGallery();
            }
            else if (method == "POST" && path.StartsWith("/updateimage"))
            {
                await galleriesService.UpdateImage();
            }
            else if (method == "POST" && path.StartsWith("/getimage"))
            {
                await galleriesService.GetImage();
            }
            else if (method == "POST" && path.StartsWith("/removeinvitation"))
            {
                await viewGallaryService.RemoveInvitation();
            }
            else if (method == "POST" && path == "/deletegallery")
            {
                await galleriesService.DeleteGallery();
            }
            else if (method == "POST" && path == "/editimage") // Добавлено для редактирования галереи
            {
                await galleriesService.EditGallery();
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




}




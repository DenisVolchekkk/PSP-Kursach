using ClientApp.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientApp.Models;
using System.Net;
using System.Web;

namespace ClientApp.Servecies
{
    public class GalleriesService : IBaseService
    {
        public JsonFileManager fileManager { get; set; }
        public HttpListenerContext context { get; set; }
        public List<User> registeredUsers { get; set; }

        public GalleriesService(HttpListenerContext Context, JsonFileManager FileManager, List<User> RegisteredUsers)
        {
            context = Context;
            fileManager = FileManager;
            registeredUsers = RegisteredUsers;
        }

        public async Task AddImage()
        {
            try
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string json = await reader.ReadToEndAsync();
                var request = JsonConvert.DeserializeObject<ImageRequest>(json);

                if (request == null || string.IsNullOrWhiteSpace(request.Username) || request.GalleryId <= 0)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Invalid image data or gallery ID"));
                    return;
                }

                var user = registeredUsers.FirstOrDefault(u => u.Username == request.Username);
                if (user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("User not found"));
                    return;
                }

                var gallery = user.Galleries.FirstOrDefault(g => g.Id == request.GalleryId);
                if (gallery == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Gallery not found"));
                    return;
                }

                var imageData = new ImageData
                {
                    Name = request.Name,
                    Description = request.Description,
                    Image = request.Image,
                    Username = request.Username
                };

                gallery.Images.Add(imageData);
                fileManager.SaveToFile();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Image added successfully"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка добавления изображения: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Internal server error"));
            }
        }
        public async Task UpdateImage()
        {
            try
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string json = await reader.ReadToEndAsync();
                var request = JsonConvert.DeserializeObject<ImageEditRequest>(json);

                if (request == null || string.IsNullOrWhiteSpace(request.Username) || request.GalleryId <= 0 || request.Name == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Invalid image data or gallery ID"));
                    return;
                }

                var user = registeredUsers.FirstOrDefault(u => u.Username == request.Username);
                if (user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("User not found"));
                    return;
                }

                var gallery = user.Galleries.FirstOrDefault(g => g.Id == request.GalleryId);
                if (gallery == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Gallery not found"));
                    return;
                }

                var image = gallery.Images.FirstOrDefault(i => i.Id == request.ImageId);
                if (image == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Image not found"));
                    return;
                }
                image.Name = request.Name ?? image.Name;
                image.Description = request.Description ?? image.Description;
                image.Image = request.Image ?? image.Image;

                fileManager.SaveToFile();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Image updated successfully"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления изображения: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Internal server error"));
            }
        }
        public async Task GetImage()
        {
            try
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string json = await reader.ReadToEndAsync();
                var request = JsonConvert.DeserializeObject<ImageEditRequest>(json);

                if (request == null || string.IsNullOrWhiteSpace(request.Username) || request.GalleryId <= 0 || request.ImageId <= 0)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Invalid image data or gallery ID"));
                    return;
                }

                var user = registeredUsers.FirstOrDefault(u => u.Username == request.Username);
                if (user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("User not found"));
                    return;
                }

                var gallery = user.Galleries.FirstOrDefault(g => g.Id == request.GalleryId);
                if (gallery == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Gallery not found"));
                    return;
                }

                var image = gallery.Images.FirstOrDefault(i => i.Id == request.ImageId);
                if (image == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Image not found"));
                    return;
                }

                // Return image metadata as a JSON response
                var imageDataResponse = new
                {
                    Name = image.Name,
                    Description = image.Description,
                    Image = image.Image  // Optional: Base64 string of the image itself (if needed)
                };

                string jsonResponse = JsonConvert.SerializeObject(imageDataResponse);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(jsonResponse));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving image: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Internal server error"));
            }
        }
        public async Task DeleteImage()
        {
            try
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string json = await reader.ReadToEndAsync();
                var request = JsonConvert.DeserializeObject<ImageEditRequest>(json);

                if (request == null || string.IsNullOrWhiteSpace(request.Username) || request.GalleryId <= 0 || request.ImageId <= 0)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Invalid image data or gallery ID"));
                    return;
                }

                var user = registeredUsers.FirstOrDefault(u => u.Username == request.Username);
                if (user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("User not found"));
                    return;
                }

                var gallery = user.Galleries.FirstOrDefault(g => g.Id == request.GalleryId);
                if (gallery == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Gallery not found"));
                    return;
                }

                var image = gallery.Images.FirstOrDefault(i => i.Id == request.ImageId);
                if (image == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Image not found"));
                    return;
                }

                gallery.Images.Remove(image);
                fileManager.SaveToFile();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Image deleted successfully"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления изображения: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Internal server error"));
            }
        }


        public async Task GetGalleries()
        {
            try
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string json = await reader.ReadToEndAsync();
                var request = JsonConvert.DeserializeObject<UserRequest>(json); // Предполагается наличие UserRequest

                if (request == null || string.IsNullOrWhiteSpace(request.Username))
                {
                    context.Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Invalid request"));
                    return;
                }

                var user = registeredUsers.FirstOrDefault(u => u.Username == request.Username);
                if (user == null)
                {
                    context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("User not found"));
                    return;
                }

                var galleries = user.Galleries;
                var responseJson = JsonConvert.SerializeObject(galleries);

                context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes(responseJson));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения галерей: {ex.Message}");
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Internal server error"));
            }
        }
        public async Task CreateGallery()
        {
            using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
            string body = await reader.ReadToEndAsync();
            var createGalleryRequest = JsonConvert.DeserializeObject<CreateGalleryRequest>(body);

            var user = registeredUsers.FirstOrDefault(u => u.Username == createGalleryRequest.Username);

            if (user != null)
            {
                var newGallery = new Gallery
                {
                    Name = createGalleryRequest.GalleryName,
                    Images = new List<ImageData>()
                };

                user.Galleries.Add(newGallery);
                fileManager.SaveToFile(); // Сохранение изменений
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }

        }
        public async Task DeleteGallery()
        {
            using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
            string body = await reader.ReadToEndAsync();
            var deleteGalleryRequest = JsonConvert.DeserializeObject<DeleteGalleryRequest>(body);

            var user = registeredUsers.FirstOrDefault(u => u.Username == deleteGalleryRequest.Username);

            if (user != null)
            {
                var gallery = user.Galleries.FirstOrDefault(g => g.Id == deleteGalleryRequest.GalleryId);

                if (gallery != null)
                {
                    user.Galleries.Remove(gallery);
                    fileManager.SaveToFile(); // Сохранение изменений
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound; // Галерея не найдена
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound; // Пользователь не найден
            }

        }

        public async Task ServeUploadImagePage()
        {
            try
            {
                // Извлечение параметра galleryId из строки запроса
                var queryParams = HttpUtility.ParseQueryString(context.Request.Url.Query);
                if (!int.TryParse(queryParams["galleryId"], out int galleryId))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Invalid galleryId"));
                    return;
                }

                // Проверяем существование галереи
                var gallery = registeredUsers
                    .SelectMany(user => user.Galleries)
                    .FirstOrDefault(g => g.Id == galleryId);

                if (gallery == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Gallery not found"));
                    return;
                }

                // Отправляем страницу загрузки изображений
                await ServeHtmlFile(context, "upload_image.html");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error serving upload_image.html: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Internal server error"));
            }
        }
        public async Task EditGallery()
        {
            using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
            string body = await reader.ReadToEndAsync();
            var editGalleryRequest = JsonConvert.DeserializeObject<EditGalleryRequest>(body);

            if (editGalleryRequest == null || string.IsNullOrWhiteSpace(editGalleryRequest.Username))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Invalid request"));
                return;
            }

            var user = registeredUsers.FirstOrDefault(u => u.Username == editGalleryRequest.Username);

            if (user == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("User not found"));
                return;
            }

            var gallery = user.Galleries.FirstOrDefault(g => g.Id == editGalleryRequest.GalleryId);

            if (gallery == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Gallery not found"));
                return;
            }

            gallery.Name = editGalleryRequest.NewName;
            fileManager.SaveToFile();

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Gallery updated successfully"));
        }

        public async Task ServeHtmlFile(HttpListenerContext context, string filePath)
        {
            await ServeStaticFile(context, filePath, "text/html");
        }
        public async Task ServeStaticFile(HttpListenerContext context, string filePath, string contentType)
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

    }

}

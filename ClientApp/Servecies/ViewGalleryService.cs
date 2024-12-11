using ClientApp.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientApp.Models;
using System.Net;


namespace ClientApp.Servecies
{
    public class ViewGalleryService : IBaseService
    {
        public JsonFileManager fileManager { get; set; }
        public HttpListenerContext context { get; set; }
        public List<User> registeredUsers { get; set; }

        public ViewGalleryService(HttpListenerContext Context, JsonFileManager FileManager, List<User> RegisteredUsers)
        {
            context = Context;
            fileManager = FileManager;
            registeredUsers = RegisteredUsers;
        }

        public async Task GetUserGallery()
        {
            string username = context.Request.QueryString["username"];
            int.TryParse(context.Request.QueryString["galleryId"], out int galleryId);

            var user = registeredUsers.FirstOrDefault(u => u.Username == username);
            var gallery = user?.Galleries.FirstOrDefault(g => g.Id == galleryId);

            if (gallery != null)
            {
                var galleryJson = JsonConvert.SerializeObject(gallery);
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

        public async Task InviteUser()
        {
            string username = context.Request.QueryString["username"];
            string invitee = context.Request.QueryString["invitee"];
            int.TryParse(context.Request.QueryString["galleryId"], out int galleryId);

            var user = registeredUsers.FirstOrDefault(u => u.Username == username);
            var invitedUser = registeredUsers.FirstOrDefault(u => u.Username == invitee);
            var gallery = user?.Galleries.FirstOrDefault(g => g.Id == galleryId);

            if (user != null && invitedUser != null && gallery != null)
            {
                user.InvitedUsers.Add(invitee);
                invitedUser.Invitations.Add($"{username}|{galleryId}"); // Сохраняем ссылку на галерею
                fileManager.SaveToFile(); // Сохранение изменений
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }

        public async Task GetInvitations()
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

        public async Task AcceptInvitation()
        {
            string username = context.Request.QueryString["username"];
            string invitation = context.Request.QueryString["invitation"]; // Формат: "inviter|galleryId"
            int.TryParse(context.Request.QueryString["galleryId"], out int galleryId);
            var user = registeredUsers.FirstOrDefault(u => u.Username == username);
            if (user != null && !string.IsNullOrWhiteSpace(invitation))
            {

                var inviter = registeredUsers.FirstOrDefault(u => u.Username == invitation);
                var gallery = inviter?.Galleries.FirstOrDefault(g => g.Id == galleryId);

                if (gallery != null)
                {
                    user.InvitedUsers.Add(inviter.Username);
                    fileManager.SaveToFile(); // Сохранение изменений
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    return;
                }
                
            }

            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        }
        public async Task RemoveInvitation()
        {
            string username = context.Request.QueryString["username"];
            string inviter = context.Request.QueryString["inviter"];
            int.TryParse(context.Request.QueryString["galleryId"], out int galleryId);

            var user = registeredUsers.FirstOrDefault(u => u.Username == username);
            var inviterUser = registeredUsers.FirstOrDefault(u => u.Username == inviter);

            if (user != null && inviterUser != null)
            {
                string invitation = $"{inviter}|{galleryId}";

                // Удаляем приглашение у обоих пользователей
                user.Invitations.Remove(invitation);
                inviterUser.InvitedUsers.Remove(username);

                fileManager.SaveToFile(); // Сохранение изменений
                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }

        public async Task LikeImage()
        {
            try
            {
                using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
                var json = await reader.ReadToEndAsync();

                var likeData = JsonConvert.DeserializeObject<LikeRequest>(json);

                var username = likeData.Username; // Текущий пользователь
                var galleryId = likeData.GalleryId;
                var imageName = likeData.ImageName;
                var inviteUsername = likeData.InviteUsername;

                var user = registeredUsers.FirstOrDefault(u => u.Username == username);
                var gallery = user?.Galleries.FirstOrDefault(g => g.Id == galleryId);
                var image = gallery?.Images.FirstOrDefault(img => img.Name == imageName);

                if (image != null)
                {
                    if (image.LikedBy.Contains(inviteUsername))
                    {
                        // Пользователь уже поставил лайк
                        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                        await context.Response.OutputStream.WriteAsync(
                            Encoding.UTF8.GetBytes("You have already liked this image."));
                    }
                    else if (image.LikedBy.Contains(username) && inviteUsername == null)
                    {
                        // Пользователь уже поставил лайк
                        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                        await context.Response.OutputStream.WriteAsync(
                            Encoding.UTF8.GetBytes("You have already liked this image."));
                    }
                    else
                    {
                        // Добавляем пользователя в список LikedBy
                        if(inviteUsername!= null)
                        {
                            image.LikedBy.Add(inviteUsername);
                            image.Likes++;

                            // Сохраняем изменения
                            fileManager.SaveToFile();

                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                        }
                        else
                        {
                            image.LikedBy.Add(username);
                            image.Likes++;

                            // Сохраняем изменения
                            fileManager.SaveToFile();

                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                        }

                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки лайка: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }



        public async Task AddComment()
        {
            try
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string body = await reader.ReadToEndAsync();
                var commentRequest = JsonConvert.DeserializeObject<CommentRequest>(body);

                var user = registeredUsers.FirstOrDefault(u => u.Username == commentRequest.GalleryOwner);
                var gallery = user?.Galleries.FirstOrDefault(g => g.Id == commentRequest.GalleryId);
                var image = gallery?.Images.FirstOrDefault(img => img.Name == commentRequest.ImageName);

                if (image != null)
                {
                    var comment = new Comment
                    {
                        Username = commentRequest.Commenter,
                        Text = commentRequest.CommentText
                    };
                    image.Comments.Add(comment);
                    fileManager.SaveToFile(); // Сохранение изменений
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка добавления комментария: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

    }

}

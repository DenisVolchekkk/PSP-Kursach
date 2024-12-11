using ClientApp.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ClientApp.Models;
using System.Net;

namespace ClientApp.Servecies
{
    public class RegisterService : IBaseService
    {
        public JsonFileManager fileManager { get; set; }
        public HttpListenerContext context { get; set; }
        public List<User> registeredUsers { get; set; }

        public RegisterService(HttpListenerContext Context, JsonFileManager FileManager, List<User> RegisteredUsers)
        {
            context = Context;
            fileManager = FileManager;
            registeredUsers = RegisteredUsers;
        }

        public async Task RegisterUser()
        {
            using var reader = new StreamReader(context.Request.InputStream);
            var json = await reader.ReadToEndAsync();
            var userData = JsonConvert.DeserializeObject<User>(json);

            if (registeredUsers.Any(u => u.Username == userData.Username))
            {
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.Conflict;
            }
            else
            {
                // Хэшируем пароль
                using var sha256 = SHA256.Create();
                var passwordHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(userData.PasswordHash)));
                userData.PasswordHash = passwordHash;

                registeredUsers.Add(userData);
                fileManager.SaveToFile();
                context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
            }
        }

        public async Task LoginUser()
        {
            try
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                var json = await reader.ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<User>(json);

                if (data == null || string.IsNullOrWhiteSpace(data.Username) || string.IsNullOrWhiteSpace(data.PasswordHash))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                var user = registeredUsers.FirstOrDefault(u => u.Username == data.Username);
                if (user != null)
                {
                    using var sha256 = SHA256.Create();
                    var passwordHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(data.PasswordHash)));

                    if (user.PasswordHash == passwordHash)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    }
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

    }
}

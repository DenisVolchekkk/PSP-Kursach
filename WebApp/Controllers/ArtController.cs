using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Text;

namespace WebApp.Controllers
{
    public class ArtController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SendMessage(string message)
        {
            try
            {
                // Подключаемся к TCP-серверу
                TcpClient client = new TcpClient("127.0.0.1", 12345);
                NetworkStream stream = client.GetStream();

                // Отправляем сообщение на сервер
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // Получаем ответ от сервера
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                client.Close();
                ViewBag.Response = response;
            }
            catch (Exception ex)
            {
                ViewBag.Response = $"Ошибка: {ex.Message}";
            }

            return View("Index");
        }
    }
}

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

class ServerApp
{
    const int Port = 8080;
    const string ImageDirectory = "UploadedImages";

    static async Task Main()
    {
        if (!Directory.Exists(ImageDirectory))
        {
            Directory.CreateDirectory(ImageDirectory);
        }

        TcpListener listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();
        Console.WriteLine("Сервер запущен. Ожидание подключений...");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Подключен клиент.");

            Task.Run(() => HandleClient(client));
        }
    }

    static async Task HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true))
        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            try
            {
                // Получаем название и описание изображения
                string imageName = reader.ReadString();
                string imageDescription = reader.ReadString();

                // Получаем изображение в виде массива байтов
                int imageSize = reader.ReadInt32();
                byte[] imageData = reader.ReadBytes(imageSize);

                // Обработка имени файла для безопасного сохранения
                string safeFileName = SanitizeFileName(imageName);
                string filePath = Path.Combine(ImageDirectory, safeFileName);

                // Сохраняем изображение на сервере
                await File.WriteAllBytesAsync(filePath, imageData);

                // Отправляем подтверждение обратно на WebSocket-сервер
                writer.Write($"Изображение {safeFileName} получено и сохранено.");

                Console.WriteLine($"Изображение {safeFileName} сохранено с описанием: {imageDescription}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }
    }

    // Функция для санитизации имени файла
    static string SanitizeFileName(string fileName)
    {
        // Заменить пробелы и специальные символы на безопасные
        return fileName.Replace(" ", "_")
                       .Replace("\\", "_")
                       .Replace("/", "_")
                       .Replace(":", "_");
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientApp.Models;

namespace ClientApp.Interfaces
{
    // Интерфейс для работы с файлами JSON
    public interface IJsonFileManager
    {
        List<User> LoadFromFile();
        void SaveToFile();
    }

}

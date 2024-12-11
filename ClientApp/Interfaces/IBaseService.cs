using ClientApp.Servecies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ClientApp.Models;

namespace ClientApp.Interfaces
{
    public interface IBaseService
    {
        JsonFileManager fileManager { get; set; }
        HttpListenerContext context { get; set; }
        List<User> registeredUsers { get; set; }
    }
}

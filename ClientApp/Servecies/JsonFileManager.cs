using ClientApp.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientApp.Models;

namespace ClientApp.Servecies
{
    public class JsonFileManager : IJsonFileManager
    {
        private  string usersFilePath { get; set; }
        private  List<User> registeredUsers { get; set; }
        public  JsonFileManager(string UsersFilePath, List<User> RegisteredUsers) 
        { 
            usersFilePath = UsersFilePath;
            registeredUsers = RegisteredUsers;
        }

        public  List<User> LoadFromFile()
        {
            if (File.Exists(usersFilePath))
            {
                var json = File.ReadAllText(usersFilePath);
                registeredUsers = JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
            }
            else
            {
                registeredUsers = new List<User>();
            }
            return registeredUsers;
        }

        public void SaveToFile()
        {
            var json = JsonConvert.SerializeObject(registeredUsers, Formatting.Indented);
            File.WriteAllText(usersFilePath, json);
        }
    }
}

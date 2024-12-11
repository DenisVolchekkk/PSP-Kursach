using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Models
{
    public class User
    {
        private static int _idCounter = 1;

        public int Id { get; private set; } = _idCounter++;
        public string Username { get; set; }
        public string PasswordHash { get; set; } // Новый хэш пароля
        public List<Gallery> Galleries { get; set; } = new List<Gallery>();
        public List<string> InvitedUsers { get; set; } = new List<string>();
        public List<string> Invitations { get; set; } = new List<string>();
    }


}

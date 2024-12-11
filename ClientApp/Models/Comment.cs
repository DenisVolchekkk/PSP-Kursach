using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Models
{

    public class Comment
    {
        private static int _idCounter = 1;

        public int Id { get; private set; } = _idCounter++;
        public string Username { get; set; }
        public string Text { get; set; }
    }
}

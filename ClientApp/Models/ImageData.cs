using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Models
{
    public class ImageData
    {
        private static int _idCounter = 1;

        public int Id { get; private set; } = _idCounter++;
        public string Username { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int Likes { get; set; }
        public HashSet<string> LikedBy { get; set; } = new HashSet<string>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}

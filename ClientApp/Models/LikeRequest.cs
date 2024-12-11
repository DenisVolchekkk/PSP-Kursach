using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Models
{
    public class LikeRequest
    {
        public string Username { get; set; }
        public int GalleryId { get; set; }
        public string ImageName { get; set; }
        public string InviteUsername { get; set; }
    }

}

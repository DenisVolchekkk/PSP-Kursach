using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Models
{
    public class EditGalleryRequest
    {
        public string Username { get; set; } // Имя пользователя, которому принадлежит галерея
        public int GalleryId { get; set; } // Идентификатор галереи
        public string NewName { get; set; } // Новое имя галереи
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Models
{
    public class DeleteGalleryRequest
    {
        /// <summary>
        /// Имя пользователя, чья галерея будет удалена.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Уникальный идентификатор галереи, которую нужно удалить.
        /// </summary>
        public int GalleryId { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Models
{
    public class UserRequest
    {
        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Уникальный идентификатор галереи (опционально).
        /// </summary>
        public int? GalleryId { get; set; }
    }



}

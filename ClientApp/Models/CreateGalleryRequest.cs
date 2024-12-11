using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Models
{
    public class CreateGalleryRequest
    {
        /// <summary>
        /// Имя пользователя, создающего галерею.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Название новой галереи.
        /// </summary>
        public string GalleryName { get; set; }

        /// <summary>
        /// Описание галереи (опционально).
        /// </summary>
        public string Description { get; set; }
    }
}

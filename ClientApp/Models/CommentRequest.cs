using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp.Models
{
    public class CommentRequest
    {
        /// <summary>
        /// Имя пользователя, владеющего галереей.
        /// </summary>
        public string GalleryOwner { get; set; }

        /// <summary>
        /// Идентификатор галереи, содержащей изображение.
        /// </summary>
        public int GalleryId { get; set; }

        /// <summary>
        /// Имя изображения, к которому добавляется комментарий.
        /// </summary>
        public string ImageName { get; set; }

        /// <summary>
        /// Имя пользователя, оставляющего комментарий.
        /// </summary>
        public string Commenter { get; set; }

        /// <summary>
        /// Текст комментария.
        /// </summary>
        public string CommentText { get; set; }
    }

}

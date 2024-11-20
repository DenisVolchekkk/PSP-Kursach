using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    class User
    {
        public string Username { get; set; }
        public List<ImageData> Gallery { get; set; } = new List<ImageData>();
        public List<string> InvitedUsers { get; set; } = new List<string>();
        public List<string> Invitations { get; set; } = new List<string>();
    }

    class ImageData
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int Likes { get; set; }
        public HashSet<string> LikedBy { get; set; } = new HashSet<string>();
        public List<Comment> Comments { get; set; } = new List<Comment>(); // Новый список для комментариев
    }
    class CommentRequest
    {
        public string ImageOwner { get; set; }
        public string ImageName { get; set; }
        public string Commenter { get; set; }
        public string CommentText { get; set; }
    }


    class LikeRequest
    {
        public string Username { get; set; }
        public string ImageName { get; set; }
    }
    class Comment
    {
        public string Username { get; set; }
        public string Text { get; set; }
    }

}

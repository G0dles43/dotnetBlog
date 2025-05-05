using System.ComponentModel.DataAnnotations;
using BlogApp.Models;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string UserId { get; set; } = string.Empty; // Autor komentarza
        public IdentityUser User { get; set; }

        public int PostId { get; set; } // Powiązanie z postem
        public Post Post { get; set; } = null!;

        public string? ImagePath { get; set; }

        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;
        public virtual ICollection<CommentVote> Votes { get; set; } = new List<CommentVote>();
    }
}

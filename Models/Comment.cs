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

        public string UserId { get; set; } = string.Empty;
        public IdentityUser User { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        public byte[]? ImageData { get; set; }
        public string? ImageMimeType { get; set; }

        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;
        public virtual ICollection<CommentVote> Votes { get; set; } = new List<CommentVote>();
    }
}

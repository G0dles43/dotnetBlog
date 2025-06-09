using System.Collections.Generic;
using BlogApp.Models;
using System.ComponentModel.DataAnnotations.Schema;


namespace BlogApp.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int BlogId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int ViewCount { get; set; } = 0;
        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;
        public string UserId { get; set; } = string.Empty;
        public virtual ICollection<PostVote> Votes { get; set; } = new List<PostVote>();
        public byte[]? ImageData { get; set; }
        public string? ImageMimeType { get; set; }
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        [NotMapped]
        public double? AverageRating { get; set; }

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public bool IsPrivate { get; set; } = false;
        public string? AccessPassword { get; set; }

    }
}

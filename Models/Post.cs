using System.Collections.Generic;
using BlogApp.Models;

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
        public string? ImagePath { get; set; }
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}

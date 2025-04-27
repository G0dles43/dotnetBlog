using System.Collections.Generic;

namespace BlogApp.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public int BlogId { get; set; }
        public Blog Blog { get; set; }

        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();

        // Dodaj brakujące właściwości:
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int ViewCount { get; set; } = 0;
        public int Likes { get; set; } = 0;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}

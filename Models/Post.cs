using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int BlogId { get; set; } // Powiązanie z Blogiem
        public Blog Blog { get; set; } = null!;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public int ViewCount { get; set; } = 0; // licznik wyświetleń

        public int Likes { get; set; } = 0; // licznik polubień
    }
}

using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string UserId { get; set; } = string.Empty; // Autor komentarza

        public int PostId { get; set; } // Powiązanie z postem
        public Post Post { get; set; } = null!;

        public string? ImagePath { get; set; }
        public int Rating { get; set; } = 0; // Ocenianie komentarzy
    }
}

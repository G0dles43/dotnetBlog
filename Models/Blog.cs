using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Blog
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty; 

        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}

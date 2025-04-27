using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.Models
{
    public class Profile
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public IdentityUser User { get; set; } = null!;

        public DateTime DateOfRegistration { get; set; } = DateTime.UtcNow;
    }
}

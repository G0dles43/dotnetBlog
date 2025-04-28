using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models  
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Username")]
        public string DisplayName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}

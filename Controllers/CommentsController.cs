using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using BlogApp.Data;
using BlogApp.Models;


namespace BlogApp.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly AppDbContext _context;
        public CommentsController(AppDbContext c) => _context = c;

        [HttpPost]
        public async Task<IActionResult> Create(int postId, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return RedirectToAction("Details", "Posts", new { id = postId });

            var comment = new Comment {
                PostId = postId,
                Content = content,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Posts", new { id = postId });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BlogApp.Data;
using BlogApp.Models;

namespace BlogApp.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;
        public ReportsController(AppDbContext context) => _context = context;

        [HttpPost]
        public async Task<IActionResult> Create(int? postId, int? commentId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                if (postId.HasValue)
                    return RedirectToAction("Details", "Posts", new { id = postId });
                if (commentId.HasValue)
                {
                    var comment = await _context.Comments.Include(c => c.Post).FirstOrDefaultAsync(c => c.Id == commentId);
                    if (comment != null)
                        return RedirectToAction("Details", "Posts", new { id = comment.PostId });
                }

                return RedirectToAction("Index", "Home");
            }

            var report = new Report
            {
                Content = content,
                CreatedAt = DateTime.UtcNow,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                PostId = postId,
                CommentId = commentId
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            if (postId.HasValue)
                return RedirectToAction("Details", "Posts", new { id = postId });

            if (commentId.HasValue)
            {
                var comment = await _context.Comments.Include(c => c.Post).FirstOrDefaultAsync(c => c.Id == commentId);
                if (comment != null)
                    return RedirectToAction("Details", "Posts", new { id = comment.PostId });
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var reports = await _context.Reports
                .Include(r => r.User)
                .Include(r => r.Post)
                .Include(r => r.Comment)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reports);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? postId = null, int? commentId = null)
        {
            ViewBag.CommentId = commentId;

            if (postId.HasValue)
            {
                ViewBag.PostId = postId;
            }
            else if (commentId.HasValue)
            {
                var comment = await _context.Comments.Include(c => c.Post)
                                                    .FirstOrDefaultAsync(c => c.Id == commentId);
                if (comment != null)
                {
                    ViewBag.PostId = comment.PostId;
                }
            }

            return View();
        }
    }
}

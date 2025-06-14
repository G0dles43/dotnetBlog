using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using BlogApp.Data;
using BlogApp.Models;
using SixLabors.ImageSharp;          
using SixLabors.ImageSharp.Processing; 

namespace BlogApp.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly AppDbContext _context;
        public CommentsController(AppDbContext c) => _context = c;

        [HttpPost]
        public async Task<IActionResult> Create(int postId, string content, IFormFile? imageFile)
        {
            if (string.IsNullOrWhiteSpace(content)) return RedirectToAction("Details", "Posts", new { id = postId });

            var comment = new Comment
            {
                PostId = postId,
                Content = content,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            if (imageFile != null && imageFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await imageFile.CopyToAsync(memoryStream);
                comment.ImageData = memoryStream.ToArray();
                comment.ImageMimeType = imageFile.ContentType;
            }

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Posts", new { id = postId });
        }

        private async Task<string> ProcessImage(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine("wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var image = await SixLabors.ImageSharp.Image.LoadAsync(imageFile.OpenReadStream()))
            {
                image.Mutate(x => x.Resize(800, 600));
                await image.SaveAsync(filePath);
            }

            return "/uploads/" + uniqueFileName;
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            return View(comment);
        }

        // POST: Comments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string content, IFormFile? imageFile)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();


            if (ModelState.IsValid)
            {
                comment.Content = content;

                if (imageFile != null && imageFile.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await imageFile.CopyToAsync(memoryStream);
                    comment.ImageData = memoryStream.ToArray();
                    comment.ImageMimeType = imageFile.ContentType;
                }

                _context.Update(comment);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Posts", new { id = comment.PostId });
            }

            return View(comment);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(m => m.Id == id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment == null || (comment.UserId != userId && !User.IsInRole("Admin")))
                return Forbid();
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                var postId = comment.PostId;
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Posts", new { id = postId });
            }
            return NotFound();
        }
        
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentImage(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment?.ImageData == null || comment.ImageMimeType == null)
                return NotFound();

            return File(comment.ImageData, comment.ImageMimeType);
        }

        
    }
}

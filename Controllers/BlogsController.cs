using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BlogApp.Data;
using BlogApp.Models;
using System.Security.Claims;

namespace BlogApp.Controllers
{
    [Authorize]
    public class BlogsController : Controller
    {
        private readonly AppDbContext _context;

        public BlogsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Blogs
        public async Task<IActionResult> Index()
        {
            return View(await _context.Blogs.ToListAsync());
        }

        // GET: Blogs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Blogs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description")] Blog blog)
        {
            if (ModelState.IsValid)
            {
                blog.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Ustawiamy przed dodaniem
                _context.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(blog);
        }

        // GET: Blogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var blog = await _context.Blogs.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (blog == null || (blog.UserId != userId && !User.IsInRole("Admin")))
                return Forbid();

            return View(blog);
        }

        // POST: Blogs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description")] Blog updatedBlog)
        {
            if (id != updatedBlog.Id)
                return NotFound();

            var blogInDb = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (blogInDb == null || (blogInDb.UserId != userId && !User.IsInRole("Admin")))
                return Forbid();

            if (ModelState.IsValid)
            {
                blogInDb.Title = updatedBlog.Title;
                blogInDb.Description = updatedBlog.Description;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Blogs.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(updatedBlog);
        }

        // GET: Blogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var blog = await _context.Blogs.FirstOrDefaultAsync(m => m.Id == id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (blog == null || (blog.UserId != userId && !User.IsInRole("Admin")))
                return Forbid();

            return View(blog);
        }

        // POST: Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (blog == null || (blog.UserId != userId && !User.IsInRole("Admin")))
                return Forbid();

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Blogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Posts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }
    }
}

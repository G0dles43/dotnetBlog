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
        public async Task<IActionResult> Index(string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParam = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.PostsSortParam = sortOrder == "posts" ? "posts_desc" : "posts";

            var blogs = _context.Blogs.Include(b => b.Posts).AsQueryable();

            switch (sortOrder)
            {
                case "name_desc":
                    blogs = blogs.OrderByDescending(b => b.Title);
                    break;
                case "posts":
                    blogs = blogs.OrderBy(b => b.Posts.Count);
                    break;
                case "posts_desc":
                    blogs = blogs.OrderByDescending(b => b.Posts.Count);
                    break;
                default: 
                    blogs = blogs.OrderBy(b => b.Title);
                    break;
            }

            return View(await blogs.ToListAsync());
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
        public async Task<IActionResult> Details(int? id, string? sortOrder, int? tagId)
        {
            if (id == null) return NotFound();

            var blog = await _context.Blogs
                .Include(b => b.Posts)
                    .ThenInclude(p => p.PostTags)
                        .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null) return NotFound();

            ViewBag.NameSortParam = sortOrder == "name_desc" ? "name" : "name_desc";
            ViewBag.ViewsSortParam = sortOrder == "views_desc" ? "views" : "views_desc";
            ViewBag.CurrentSort = sortOrder;
            var posts = blog.Posts.AsQueryable();

            if (tagId.HasValue)
            {
                posts = posts.Where(p => p.PostTags.Any(pt => pt.TagId == tagId.Value));
                ViewBag.SelectedTag = await _context.Tags.FindAsync(tagId.Value);
            }

            switch (sortOrder)
            {
                case "name_desc":
                    posts = posts.OrderByDescending(p => p.Title);
                    break;
                case "views":
                    posts = posts.OrderBy(p => p.ViewCount);
                    break;
                case "views_desc":
                    posts = posts.OrderByDescending(p => p.ViewCount);
                    break;
                default:
                    posts = posts.OrderBy(p => p.Title);
                    break;
            }

            blog.Posts = posts.ToList(); 

            return View(blog);
        }
    }
}

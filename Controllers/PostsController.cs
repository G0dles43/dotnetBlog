using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BlogApp.Data;
using BlogApp.Models;
using SixLabors.ImageSharp;          
using SixLabors.ImageSharp.Processing; 
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;


namespace BlogApp.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly AppDbContext _context;

        public PostsController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.Posts
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)  // Ładujemy dane użytkownika powiązane z komentarzami
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            // Opcjonalnie zwiększ licznik wyświetleń
            post.ViewCount++;
            await _context.SaveChangesAsync();

            return View(post);
        }


        // GET: Posts
        public async Task<IActionResult> Index()
        {
            return View(await _context.Posts.ToListAsync());
        }

        // GET: Posts/Create
        public IActionResult Create(int? blogId)
        {
            if (!blogId.HasValue)
            {
                return BadRequest("BlogId is required.");
            }

            var post = new Post { BlogId = blogId.Value };
            return View(post);
        }

        // POST: Posts/Create
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,BlogId")] Post post, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    post.ImagePath = await ProcessImage(imageFile);  
                }
                post.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Blogs", new { id = post.BlogId });
            }
            return View(post);
        }


        // GET: Posts/Edit/5
        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FirstOrDefaultAsync(m => m.Id == id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (post == null)
            {
                return NotFound();
            }

            // Dodaj listę blogów do ViewBag.Blogs
            ViewBag.Blogs = new SelectList(_context.Blogs, "Id", "Title", post.BlogId);

            return View(post);
        }


        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,BlogId")] Post post, IFormFile? imageFile)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Jeśli użytkownik dodał obrazek, przetwarzamy go
                if (imageFile != null && imageFile.Length > 0)
                {
                    post.ImagePath = await ProcessImage(imageFile);
                }

                try
                {
                    // Aktualizujemy post w bazie danych
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Posts.Any(e => e.Id == post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Po zapisaniu edytowanego posta przekierowujemy do szczegółów bloga
                return RedirectToAction("Details", "Blogs", new { id = post.BlogId });
            }

            return View(post);
        }
        
        private async Task<string> ProcessImage(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine("wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);
            
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var image = await Image.LoadAsync(imageFile.OpenReadStream()))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(800, 600),
                    Mode = ResizeMode.Max
                }));
                
                await image.SaveAsync(filePath);
            }

            return $"/uploads/{uniqueFileName}";
        }
        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (post == null || (post.UserId != userId && !User.IsInRole("Admin")))
                return Forbid();

            if (post != null)
            {
                var blogId = post.BlogId;  // Pobieramy BlogId powiązane z postem
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
                
                // Po usunięciu postu przekierowujemy do widoku Details bloga
                return RedirectToAction("Details", "Blogs", new { id = blogId });
            }

            return NotFound();  // Jeśli post nie istnieje, zwróć NotFound
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> VotePost(int id, bool isUpvote)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var post = await _context.Posts
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            var existingVote = post.Votes.FirstOrDefault(v => v.UserId == userId);

            if (existingVote != null)
            {
                if (existingVote.IsUpvote == isUpvote)
                {
                    // Usuń głos, jeśli ten sam
                    _context.PostVotes.Remove(existingVote);
                    if (isUpvote) post.Likes--;
                    else post.Dislikes--;
                }
                else
                {
                    // Zmień głos
                    existingVote.IsUpvote = isUpvote;
                    post.Likes += isUpvote ? 1 : -1;
                    post.Dislikes += isUpvote ? -1 : 1;
                }
            }
            else
            {
                // Nowy głos
                post.Votes.Add(new PostVote
                {
                    UserId = userId,
                    IsUpvote = isUpvote
                });
                if (isUpvote) post.Likes++;
                else post.Dislikes++;
            }

            await _context.SaveChangesAsync();
            return Ok(new { post.Likes, post.Dislikes });
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> VoteComment(int id, bool isUpvote)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comment = await _context.Comments
                .Include(c => c.Votes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();

            var existingVote = comment.Votes.FirstOrDefault(v => v.UserId == userId);

            if (existingVote != null)
            {
                if (existingVote.IsUpvote == isUpvote)
                {
                    // Usuń głos jeśli ten sam
                    _context.CommentVotes.Remove(existingVote);
                    if (isUpvote) comment.Likes--;
                    else comment.Dislikes--;
                }
                else
                {
                    // Zmień głos
                    existingVote.IsUpvote = isUpvote;
                    comment.Likes += isUpvote ? 1 : -1;
                    comment.Dislikes += isUpvote ? -1 : 1;
                }
            }
            else
            {
                // Nowy głos
                comment.Votes.Add(new CommentVote
                {
                    UserId = userId,
                    IsUpvote = isUpvote
                });
                if (isUpvote) comment.Likes++;
                else comment.Dislikes++;
            }

            await _context.SaveChangesAsync();
            return Ok(new { comment.Likes, comment.Dislikes });
        }


    }
}

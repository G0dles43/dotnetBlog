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
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);


            // Opcjonalnie zwiększ licznik wyświetleń
            post.ViewCount++;
            await _context.SaveChangesAsync();

            return View(post);
        }


        // GET: Posts
        public async Task<IActionResult> Index(int? tagId)
        {
            var postsQuery = _context.Posts
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .AsQueryable();

            if (tagId.HasValue)
            {
                postsQuery = postsQuery.Where(p => p.PostTags.Any(pt => pt.TagId == tagId.Value));
                ViewBag.SelectedTag = await _context.Tags.FindAsync(tagId.Value);
            }

            var posts = await postsQuery.ToListAsync();
            return View(posts);
        }


        // GET: Posts/Create
        public IActionResult Create(int? blogId)
        {
            if (!blogId.HasValue)
                return BadRequest("BlogId is required.");

            ViewBag.BlogId = blogId.Value;
            ViewBag.AvailableTags = _context.Tags
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name })
                .ToList();

            return View(new Post());
        }

        // POST: Posts/Create
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,BlogId")] Post post, string? newTags, List<int> selectedTags, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    post.ImagePath = await ProcessImage(imageFile);
                }

                post.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Dodaj wybrane tagi
                foreach (var tagId in selectedTags)
                {
                    post.PostTags.Add(new PostTag { TagId = tagId });
                }

                // Obsługa nowych tagów (z pola tekstowego, np. "asp.net, c#, ef")
                if (!string.IsNullOrWhiteSpace(newTags))
                {
                    var tags = newTags
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim().ToLower())
                        .Distinct();

                    foreach (var tagName in tags)
                    {
                        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == tagName);
                        if (tag == null)
                        {
                            tag = new Tag { Name = tagName };
                            _context.Tags.Add(tag);
                            await _context.SaveChangesAsync(); // Potrzebne, żeby mieć ID
                        }

                        if (!post.PostTags.Any(pt => pt.TagId == tag.Id))
                        {
                            post.PostTags.Add(new PostTag { TagId = tag.Id });
                        }
                    }
                }

                _context.Add(post);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Blogs", new { id = post.BlogId });
            }

            // Jeśli błąd - załaduj tagi ponownie
            ViewBag.AvailableTags = _context.Tags
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name })
                .ToList();

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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,BlogId,ImagePath")] Post updatedPost, IFormFile? imageFile)
        {
            if (id != updatedPost.Id)
                return NotFound();

            var postInDb = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (postInDb == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (postInDb.UserId != userId && !User.IsInRole("Admin"))
                return Forbid(); // zabezpieczenie

            // Aktualizuj tylko edytowalne pola
            postInDb.Title = updatedPost.Title;
            postInDb.Content = updatedPost.Content;
            postInDb.BlogId = updatedPost.BlogId;

            if (imageFile != null && imageFile.Length > 0)
            {
                postInDb.ImagePath = await ProcessImage(imageFile);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Posts.Any(e => e.Id == updatedPost.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction("Details", "Blogs", new { id = postInDb.BlogId });
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

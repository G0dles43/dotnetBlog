using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlogApp.Models;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Blog> Blogs { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PostVote> PostVotes { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
        public DbSet<PostViewHistory> PostViewHistories { get; set; }
        public DbSet<PostRating> PostRatings { get; set; }
        public DbSet<CommentVote> CommentVotes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PostTag>()
                .HasKey(pt => new { pt.PostId, pt.TagId });

            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Post)
                .WithMany(p => p.PostTags)
                .HasForeignKey(pt => pt.PostId);

            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.PostTags)
                .HasForeignKey(pt => pt.TagId);
        }

    }
}

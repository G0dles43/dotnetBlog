using BlogApp.Models;
public class PostViewHistory
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int PostId { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    public Post Post { get; set; }
}

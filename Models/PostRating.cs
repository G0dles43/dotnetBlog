using BlogApp.Models;

public class PostRating
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int PostId { get; set; }
    public int Rating { get; set; } 

    public Post Post { get; set; }
}

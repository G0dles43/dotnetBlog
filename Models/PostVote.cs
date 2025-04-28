public class PostVote
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int PostId { get; set; }
    public bool IsUpvote { get; set; }
}
public class CommentVote
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int CommentId { get; set; }
    public bool IsUpvote { get; set; }
}
namespace ApiTwitterUala.DTOs
{
    public class TweetViewDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? UserName { get; set; }
    }
}
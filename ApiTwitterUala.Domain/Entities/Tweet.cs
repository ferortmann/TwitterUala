namespace ApiTwitterUala.Domain.Entities
{
    public class Tweet
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid UserId { get; init; }
        public string Content { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }
}

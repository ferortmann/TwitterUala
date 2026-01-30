namespace ApiTwitterUala.Domain.Entities
{
    public class Follow
    {
        public Guid UserId { get; init; }
        public Guid UserFollowerId { get; init; }
    }
}

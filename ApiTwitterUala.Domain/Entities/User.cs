namespace ApiTwitterUala.Domain.Entities
{
    public class User
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string UserName { get; init; } = string.Empty;
    }
}

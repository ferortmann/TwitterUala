namespace ApiTwitterUala.Cache.Services.Interfaces
{
    public interface IFollowCacheService
    {
        Task<List<Guid>?> GetFollowerIdsAsync(Guid userId, CancellationToken ct = default);
        Task SetFollowerIdsAsync(Guid userId, List<Guid> followerIds, CancellationToken ct = default);
        Task InvalidateFollowersAsync(Guid userId, CancellationToken ct = default);
    }
}
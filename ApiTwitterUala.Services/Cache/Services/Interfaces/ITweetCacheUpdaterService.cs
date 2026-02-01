using ApiTwitterUala.Services.DTOs;

namespace ApiTwitterUala.Services.Cache.Services.Interfaces
{
    public interface ITweetCacheUpdaterService
    {
        void EnqueueUpdateForFollowers(TweetViewDto tweet, int pageSize);
        Task UpdateFollowersCacheAsync(TweetViewDto tweet, int pageSize, CancellationToken ct = default);
    }
}
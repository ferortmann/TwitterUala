using ApiTwitterUala.Domain.Entities;
using ApiTwitterUala.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ApiTwitterUala.Cache.Services
{
    public interface ITweetCacheService
    {
        Task<List<TweetViewDto>?> GetTweetPageAsync(int page, int pageSize, CancellationToken ct = default);
        Task SetTweetPageAsync(int page, int pageSize, List<TweetViewDto> tweets, CancellationToken ct = default);

        Task<List<TweetViewDto>?> GetUserTweetPageAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
        Task SetUserTweetPageAsync(Guid userId, int page, int pageSize, List<TweetViewDto> tweets, CancellationToken ct = default);

        Task PrependTweetAsync(TweetViewDto tweet, int pageSize, int maxPagesToInvalidate = 3, CancellationToken ct = default);
        Task PrependTweetToUserAsync(Guid userId, TweetViewDto tweet, int pageSize, int maxPagesToInvalidate = 3, CancellationToken ct = default);
        Task InvalidateAllPagesAsync(CancellationToken ct = default);
    }
}

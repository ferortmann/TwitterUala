using System.Collections.Concurrent;
using System.Text.Json;
using System.Linq;
using ApiTwitterUala.DTOs;
using Microsoft.Extensions.Caching.Distributed;

namespace ApiTwitterUala.Cache.Services
{
    public class InMemoryTweetCacheService : ITweetCacheService
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly DistributedCacheEntryOptions _cacheOptions;

        // in-process storage (key -> (serializedData, expiration))
        private static readonly ConcurrentDictionary<string, (string Data, DateTimeOffset Expiration)> _localCache = new();

        public InMemoryTweetCacheService()
        {
            _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            _cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };
        }

        private static string PageKey(int page, int pageSize) => $"tweets:page:{page}:size:{pageSize}";

        private static bool TryGetValid(string key, out string? data)
        {
            data = null;
            if (_localCache.TryGetValue(key, out var entry))
            {
                if (entry.Expiration > DateTimeOffset.UtcNow)
                {
                    data = entry.Data;
                    return true;
                }
                _localCache.TryRemove(key, out _);
            }
            return false;
        }

        public Task<List<TweetViewDto>?> GetTweetPageAsync(int page, int pageSize, CancellationToken ct = default)
        {
            var key = PageKey(page, pageSize);
            if (TryGetValid(key, out var data) && !string.IsNullOrEmpty(data))
            {
                try { return Task.FromResult(JsonSerializer.Deserialize<List<TweetViewDto>>(data, _jsonOptions)); }
                catch { return Task.FromResult<List<TweetViewDto>?>(null); }
            }
            return Task.FromResult<List<TweetViewDto>?>(null);
        }

        public Task SetTweetPageAsync(int page, int pageSize, List<TweetViewDto> tweets, CancellationToken ct = default)
        {
            var key = PageKey(page, pageSize);
            var data = JsonSerializer.Serialize(tweets, _jsonOptions);
            var ttl = _cacheOptions.AbsoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(5);
            _localCache[key] = (data, DateTimeOffset.UtcNow.Add(ttl));
            return Task.CompletedTask;
        }

        public Task PrependTweetAsync(TweetViewDto tweet, int pageSize, int maxPagesToInvalidate = 3, CancellationToken ct = default)
        {
            var page1Key = PageKey(1, pageSize);
            var ttl = _cacheOptions.AbsoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(5);

            if (TryGetValid(page1Key, out var page1Str) && !string.IsNullOrEmpty(page1Str))
            {
                var page1 = JsonSerializer.Deserialize<List<TweetViewDto>>(page1Str, _jsonOptions) ?? new List<TweetViewDto>();
                page1.Insert(0, tweet);
                if (page1.Count > pageSize)
                    page1.RemoveRange(pageSize, page1.Count - pageSize);

                var serialized = JsonSerializer.Serialize(page1, _jsonOptions);
                _localCache[page1Key] = (serialized, DateTimeOffset.UtcNow.Add(ttl));

                for (var p = 2; p <= maxPagesToInvalidate; p++)
                {
                    var k = PageKey(p, pageSize);
                    _localCache.TryRemove(k, out _);
                }
            }

            return Task.CompletedTask;
        }

        public Task<List<TweetViewDto>?> GetUserTweetPageAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
        {
            var key = $"usertweets:{userId}:page:{page}:size:{pageSize}";
            if (TryGetValid(key, out var data) && !string.IsNullOrEmpty(data))
            {
                try { return Task.FromResult(JsonSerializer.Deserialize<List<TweetViewDto>>(data, _jsonOptions)); }
                catch { return Task.FromResult<List<TweetViewDto>?>(null); }
            }
            return Task.FromResult<List<TweetViewDto>?>(null);
        }

        public Task SetUserTweetPageAsync(Guid userId, int page, int pageSize, List<TweetViewDto> tweets, CancellationToken ct = default)
        {
            var key = $"usertweets:{userId}:page:{page}:size:{pageSize}";
            var data = JsonSerializer.Serialize(tweets, _jsonOptions);
            var ttl = _cacheOptions.AbsoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(5);
            _localCache[key] = (data, DateTimeOffset.UtcNow.Add(ttl));
            return Task.CompletedTask;
        }

        public Task PrependTweetToUserAsync(Guid userId, TweetViewDto tweet, int pageSize, int maxPagesToInvalidate = 3, CancellationToken ct = default)
        {
            var page1Key = $"usertweets:{userId}:page:1:size:{pageSize}";
            var ttl = _cacheOptions.AbsoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(5);

            if (TryGetValid(page1Key, out var page1Str) && !string.IsNullOrEmpty(page1Str))
            {
                var page1 = JsonSerializer.Deserialize<List<TweetViewDto>>(page1Str, _jsonOptions) ?? new List<TweetViewDto>();
                page1.Insert(0, tweet);
                if (page1.Count > pageSize)
                    page1.RemoveRange(pageSize, page1.Count - pageSize);

                var serialized = JsonSerializer.Serialize(page1, _jsonOptions);
                _localCache[page1Key] = (serialized, DateTimeOffset.UtcNow.Add(ttl));

                for (var p = 2; p <= maxPagesToInvalidate; p++)
                {
                    var k = $"usertweets:{userId}:page:{p}:size:{pageSize}";
                    _localCache.TryRemove(k, out _);
                }
            }

            return Task.CompletedTask;
        }

        public Task InvalidateAllPagesAsync(CancellationToken ct = default)
        {
            var maxPages = 10;

            for (var p = 1; p <= maxPages; p++)
            {
                var k = PageKey(p, 30);
                _localCache.TryRemove(k, out _);
            }

            var fallbackKeys = _localCache.Keys.ToList();
            foreach (var key in fallbackKeys)
            {
                if (key.StartsWith("tweets:page:") || key.StartsWith("usertweets:"))
                {
                    _localCache.TryRemove(key, out _);
                }
            }

            return Task.CompletedTask;
        }
    }
}

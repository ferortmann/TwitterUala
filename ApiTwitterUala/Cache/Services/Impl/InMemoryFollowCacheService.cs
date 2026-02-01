using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Concurrent;

namespace ApiTwitterUala.Cache.Services
{
        public class InMemoryFollowCacheService : IFollowCacheService
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly DistributedCacheEntryOptions _cacheOptions;
        private static readonly ConcurrentDictionary<string, (string Data, DateTimeOffset Expiration)> _localCache = new();

        public InMemoryFollowCacheService()
        {
            _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            _cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };
        }

        private static string Key(Guid userId) => $"followers:{userId}";

        public Task<List<Guid>?> GetFollowerIdsAsync(Guid userId, CancellationToken ct = default)
        {
            var key = Key(userId);
            if (_localCache.TryGetValue(key, out var entry))
            {
                if (entry.Expiration > DateTimeOffset.UtcNow)
                {
                    try
                    {
                        return Task.FromResult(JsonSerializer.Deserialize<List<Guid>>(entry.Data, _jsonOptions));
                    }
                    catch { return Task.FromResult<List<Guid>?>(null); }
                }
                _localCache.TryRemove(key, out _);
            }
            return Task.FromResult<List<Guid>?>(null);
        }

        public Task SetFollowerIdsAsync(Guid userId, List<Guid> followerIds, CancellationToken ct = default)
        {
            var key = Key(userId);
            var data = JsonSerializer.Serialize(followerIds, _jsonOptions);
            var expiration = DateTimeOffset.UtcNow.Add(_cacheOptions.AbsoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(5));
            _localCache[key] = (data, expiration);
            return Task.CompletedTask;
        }

        public Task InvalidateFollowersAsync(Guid userId, CancellationToken ct = default)
        {
            var key = Key(userId);
            _localCache.TryRemove(key, out _);
            return Task.CompletedTask;
        }
    }
}
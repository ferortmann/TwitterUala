using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ApiTwitterUala.Domain.Context;
using ApiTwitterUala.Services.DTOs;
using ApiTwitterUala.Services.Cache.Services.Interfaces;
using ApiTwitterUala.Services.BackgroundTasks;
using Microsoft.EntityFrameworkCore;
using ApiTwitterUala.Cache.Services.Interfaces;

namespace ApiTwitterUala.Services.Cache.Services.Impl
{
    public class InMemoryTweetCacheUpdaterService : ITweetCacheUpdaterService
    {
        private readonly AppDbContext _context;
        private readonly ITweetCacheService _tweetCache;
        private readonly IBackgroundTaskQueue _taskQueue;
        private const int SemaphoreMax = 20;

        public InMemoryTweetCacheUpdaterService(AppDbContext context, ITweetCacheService tweetCache, IBackgroundTaskQueue taskQueue)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tweetCache = tweetCache ?? throw new ArgumentNullException(nameof(tweetCache));
            _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
        }

        public void EnqueueUpdateForFollowers(TweetViewDto tweet, int pageSize)
        {
            _taskQueue.QueueBackgroundWorkItem(async ct => await UpdateFollowersCacheAsync(tweet, pageSize, ct));
        }

        public async Task UpdateFollowersCacheAsync(TweetViewDto tweet, int pageSize, CancellationToken ct = default)
        {
            var followerIds = await _context.Follows
                .AsNoTracking()
                .Where(f => f.UserId == tweet.UserId)
                .Select(f => f.UserFollowerId)
                .ToListAsync(ct);

            using var sem = new SemaphoreSlim(SemaphoreMax);

            var tasks = followerIds.Select(async fid =>
            {
                await sem.WaitAsync(ct);
                try
                {
                    try
                    {
                        await _tweetCache.PrependTweetToUserAsync(fid, tweet, pageSize, maxPagesToInvalidate: 2, ct);
                    }
                    catch
                    {
                    }

                    try
                    {
                        var cached = await _tweetCache.GetUserTweetPageAsync(fid, 1, pageSize, ct);
                        if (cached is null)
                        {
                            var followedByFollower = await _context.Follows
                                .AsNoTracking()
                                .Where(ff => ff.UserFollowerId == fid)
                                .Select(ff => ff.UserId)
                                .ToListAsync(ct);

                            var page = await _context.Tweets
                                .AsNoTracking()
                                .Where(t => followedByFollower.Contains(t.UserId))
                                .Join(
                                    _context.Users.AsNoTracking(),
                                    t => t.UserId,
                                    u => u.Id,
                                    (t, u) => new TweetViewDto
                                    {
                                        Id = t.Id,
                                        UserId = t.UserId,
                                        Content = t.Content,
                                        CreatedAt = t.CreatedAt,
                                        UserName = u.UserName
                                    })
                                .OrderByDescending(dto => dto.CreatedAt)
                                .Take(pageSize)
                                .ToListAsync(ct);

                            try
                            {
                                await _tweetCache.SetUserTweetPageAsync(fid, 1, pageSize, page, ct);
                            }
                            catch
                            {
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                finally
                {
                    sem.Release();
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}
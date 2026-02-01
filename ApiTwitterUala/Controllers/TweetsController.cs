using ApiTwitterUala.Domain.Context;
using ApiTwitterUala.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiTwitterUala.Mappers;
using ApiTwitterUala.Cache.Services;
using ApiTwitterUala.BackgroundTasks;

namespace ApiTwitterUala.Controllers
{
    [ApiController]
    [Route("api/tweets")]
    public class TweetsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITweetCacheService? _tweetCache;
        private readonly IBackgroundTaskQueue _taskQueue;
        private const int DefaultPageSize = 30;

        public TweetsController(AppDbContext context, ITweetCacheService? tweetCache = null, IBackgroundTaskQueue? taskQueue = null)
        {
            _context = context;
            _tweetCache = tweetCache;
            _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TweetDto tweetDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userExists = await _context.Users.AsNoTracking().AnyAsync(u => u.Id == tweetDto.UserId);
            if (!userExists)
                return NotFound(new { Message = "Usuario no encontrado.", UserId = tweetDto.UserId });

            var entity = tweetDto.ToEntity();
            _context.Tweets.Add(entity);
            await _context.SaveChangesAsync();

            string? userName = null;
            try
            {
                userName = await _context.Users.AsNoTracking()
                    .Where(u => u.Id == entity.UserId)
                    .Select(u => u.UserName)
                    .FirstOrDefaultAsync();
            }
            catch
            {
            }

            var viewDto = new TweetViewDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Content = entity.Content,
                CreatedAt = entity.CreatedAt,
                UserName = userName
            };

            if (_tweetCache is not null)
            {
                // Enqueue background work to update followers' caches only (do not update creator's own cache here)
                var followerIds = await _context.Follows
                    .AsNoTracking()
                    .Where(f => f.UserId == entity.UserId)
                    .Select(f => f.UserFollowerId)
                    .ToListAsync();

                _taskQueue.QueueBackgroundWorkItem(async ct =>
                {
                    // use followerIds (already materialized) and only call cache APIs
                    using var sem = new SemaphoreSlim(20);

                    var tasks = followerIds.Select(async fid =>
                    {
                        await sem.WaitAsync(ct);
                        try
                        {
                            // Try to prepend into follower's cached timeline page 1
                            try
                            {
                                await _tweetCache.PrependTweetToUserAsync(fid, viewDto, DefaultPageSize, maxPagesToInvalidate: 2, ct);
                            }
                            catch
                            {
                                // swallow per-user prepend errors
                            }

                            // If follower has no cache (or prepend was a no-op), populate page from DB
                            try
                            {
                                var cached = await _tweetCache.GetUserTweetPageAsync(fid, 1, DefaultPageSize, ct);
                                if (cached is null)
                                {
                                    // build follower's page 1 from DB: tweets from users that follower follows
                                    var followedByFollower = await _context.Follows
                                        .AsNoTracking()
                                        .Where(ff => ff.UserFollowerId == fid)
                                        .Select(ff => ff.UserId)
                                        .ToListAsync(ct);

                                    var page = await (from t in _context.Tweets.AsNoTracking()
                                                      join u in _context.Users.AsNoTracking() on t.UserId equals u.Id
                                                      where followedByFollower.Contains(t.UserId)
                                                      orderby t.CreatedAt descending
                                                      select new TweetViewDto
                                                      {
                                                          Id = t.Id,
                                                          UserId = t.UserId,
                                                          Content = t.Content,
                                                          CreatedAt = t.CreatedAt,
                                                          UserName = u.UserName
                                                      })
                                                    .Take(DefaultPageSize)
                                                    .ToListAsync(ct);

                                    try
                                    {
                                        await _tweetCache.SetUserTweetPageAsync(fid, 1, DefaultPageSize, page, ct);
                                    }
                                    catch
                                    {
                                        // swallow cache set errors
                                    }
                                }
                            }
                            catch
                            {
                                // swallow cache check/load errors
                            }
                        }
                        finally
                        {
                            sem.Release();
                        }
                    });

                    await Task.WhenAll(tasks);
                });
            }

            return Created();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var t = await _context.Tweets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            return t is null ? NotFound() : Ok(t);
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = DefaultPageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = DefaultPageSize;

            if (_tweetCache is not null)
            {
                try
                {
                    var cached = await _tweetCache.GetUserTweetPageAsync(userId, page, pageSize);
                    if (cached is not null)
                        return Ok(cached);
                }
                catch
                {
                }
            }

              var tweetsDto = await _context.Tweets
                .AsNoTracking()
                .Join(
                    _context.Follows.AsNoTracking().Where(f => f.UserFollowerId == userId),
                    t => t.UserId,
                    f => f.UserId,
                    (t, f) => t
                )
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
                    }
                )
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (_tweetCache is not null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _tweetCache.SetUserTweetPageAsync(userId, page, pageSize, tweetsDto);
                    }
                    catch
                    {
                    }
                });
            }

            return Ok(tweetsDto);
        }
    }
}
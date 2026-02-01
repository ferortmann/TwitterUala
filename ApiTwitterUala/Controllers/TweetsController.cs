using ApiTwitterUala.Domain.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiTwitterUala.Services.Mappers;
using ApiTwitterUala.Cache.Services.Interfaces;
using ApiTwitterUala.Services.BackgroundTasks;
using ApiTwitterUala.Services.DTOs;
using ApiTwitterUala.Services.Cache.Services.Interfaces;

namespace ApiTwitterUala.Controllers
{
    [ApiController]
    [Route("api/tweets")]
    public class TweetsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITweetCacheService? _tweetCache;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ITweetCacheUpdaterService? _tweetCacheUpdater;
        private const int DefaultPageSize = 30;

        public TweetsController(AppDbContext context, ITweetCacheService? tweetCache = null, IBackgroundTaskQueue? taskQueue = null, ITweetCacheUpdaterService? tweetCacheUpdater = null)
        {
            _context = context;
            _tweetCache = tweetCache;
            _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
            _tweetCacheUpdater = tweetCacheUpdater;
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
                try
                {
                    _tweetCacheUpdater?.EnqueueUpdateForFollowers(viewDto, DefaultPageSize);
                }
                catch
                {
                }
            }

            return Created();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var t = await _context.Tweets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            return t is null ? NotFound() : Ok(t);
        }

        [HttpGet("timeline")]
        public async Task<IActionResult> TimeLine([FromQuery] Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = DefaultPageSize)
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
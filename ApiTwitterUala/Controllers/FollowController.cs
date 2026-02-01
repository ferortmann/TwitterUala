using ApiTwitterUala.Cache.Services.Interfaces;
using ApiTwitterUala.Domain.Context;
using ApiTwitterUala.Domain.Entities;
using ApiTwitterUala.Services.DTOs;
using ApiTwitterUala.Services.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace ApiTwitterUala.Controllers
{
    [ApiController]
    [Route("api/follows")]
    public class FollowController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFollowCacheService? _followCache;
        private readonly ITweetCacheService? _tweetCache;

        public FollowController(AppDbContext context, IFollowCacheService? followCache = null, ITweetCacheService? tweetCache = null)
        {
            _context = context;
            _followCache = followCache;
            _tweetCache = tweetCache;
        }

        [HttpPost]
        public async Task<IActionResult> Follow([FromBody] FollowDto followDto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var targetExists = await _context.Users.AsNoTracking().AnyAsync(u => u.Id == followDto.UserId, ct);
            if (!targetExists)
                return NotFound();

            var followerExists = await _context.Users.AsNoTracking().AnyAsync(u => u.Id == followDto.UserFollowerId, ct);
            if (!followerExists)
                return NotFound(new { Message = "Seguidor no enconrado." });

            var existing = await _context.Follows.AsNoTracking().AnyAsync(f => f.UserId == followDto.UserId && f.UserFollowerId == followDto.UserFollowerId, ct);
            if (existing)
                return Conflict(new { Message = "Ya se están siguiendo." });

            var entity = followDto.ToEntity();
            _context.Follows.Add(entity);
            try
            {
                await _context.SaveChangesAsync(ct);

                if (_followCache is not null)
                    await _followCache.InvalidateFollowersAsync(followDto.UserId, ct);

                if (_tweetCache is not null)
                    try { await _tweetCache.InvalidateUserPagesAsync(followDto.UserFollowerId, ct); } catch { }
            }
            catch (DbUpdateException ex)
            {
                return Conflict(new { Message = "Ya se están siguiendo.", Detail = ex.Message });
            }

            return Created();
        }

        [HttpDelete("{userId:guid}/{userFollowerId:guid}")]
        public async Task<IActionResult> Unfollow(Guid userId, Guid userFollowerId, CancellationToken ct)
        {
            var entity = await _context.Follows.FindAsync(new object?[] { userId, userFollowerId }, ct);
            if (entity is null)
                return NotFound();

            _context.Follows.Remove(entity);
            await _context.SaveChangesAsync(ct);

            if (_followCache is not null)
                try { await _followCache.InvalidateFollowersAsync(userId, ct); } catch { }

            if (_tweetCache is not null)
                try { await _tweetCache.InvalidateUserPagesAsync(userFollowerId, ct); } catch { }

            return NoContent();
        }

        [HttpGet("{userId:guid}/followers")]
        public async Task<ActionResult<IEnumerable<User>>> GetFollowers(Guid userId, CancellationToken ct)
        {
            List<Guid>? followerIds = null;

            if (_followCache is not null)
            {
                var cachedIds = await _followCache.GetFollowerIdsAsync(userId, ct);
                if (cachedIds is not null)
                    followerIds = cachedIds;
            }

            if (followerIds is null)
            {
                followerIds = await _context.Follows
                    .AsNoTracking()
                    .Where(f => f.UserId == userId)
                    .Select(f => f.UserFollowerId)
                    .ToListAsync(ct);

                if (_followCache is not null)
                    try { await _followCache.SetFollowerIdsAsync(userId, followerIds, ct); } catch { }
            }

            var followers = await _context.Users
                .AsNoTracking()
                .Where(u => followerIds.Contains(u.Id))
                .ToListAsync(ct);

            return Ok(followers);
        }

        [HttpGet("{userId:guid}/following")]
        public async Task<ActionResult<IEnumerable<User>>> GetFollowing(Guid userId, CancellationToken ct)
        {
            var followingIds = await _context.Follows
                .AsNoTracking()
                .Where(f => f.UserFollowerId == userId)
                .Select(f => f.UserId)
                .ToListAsync(ct);

            var following = await _context.Users
                .AsNoTracking()
                .Where(u => followingIds.Contains(u.Id))
                .ToListAsync(ct);

            return Ok(following);
        }

        [HttpGet("{userId:guid}/is-following/{targetId:guid}", Name = "IsFollowing")]
        public async Task<ActionResult<bool>> IsFollowing(Guid userId, Guid targetId, CancellationToken ct)
        {
            var exists = await _context.Follows
                .AsNoTracking()
                .AnyAsync(f => f.UserId == userId && f.UserFollowerId == targetId, ct);
            return Ok(exists);
        }
    }
}
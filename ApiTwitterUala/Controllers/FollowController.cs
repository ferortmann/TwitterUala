using ApiTwitterUala.Domain.Context;
using ApiTwitterUala.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiTwitterUala.Controllers
{
    [ApiController]
    [Route("api/follows")]
    public class FollowController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FollowController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Follow([FromBody] Follow follow)
        {
            if (follow == null)
                return BadRequest();

            if (follow.UserId == follow.UserFollowerId)
                return BadRequest("Un usuario no puede seguir a si mismo.");

            var targetExists = await _context.Users.AnyAsync(u => u.Id == follow.UserId);
            var followerExists = await _context.Users.AnyAsync(u => u.Id == follow.UserFollowerId);
            if (!targetExists || !followerExists)
                return NotFound();

            var existing = await _context.Follows.FindAsync(follow.UserId, follow.UserFollowerId);
            if (existing is not null)
                return Conflict();

            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();

            return Created();
        }

        [HttpDelete("{userId:guid}/{userFollowerId:guid}")]
        public async Task<IActionResult> Unfollow(Guid userId, Guid userFollowerId)
        {
            var entity = await _context.Follows.FindAsync(userId, userFollowerId);
            if (entity is null)
                return NotFound();

            _context.Follows.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{userId:guid}/followers")]
        public async Task<ActionResult<IEnumerable<User>>> GetFollowers(Guid userId)
        {
            var followerIds = await _context.Follows
                .Where(f => f.UserId == userId)
                .Select(f => f.UserFollowerId)
                .ToListAsync();

            var followers = await _context.Users
                .Where(u => followerIds.Contains(u.Id))
                .ToListAsync();

            return Ok(followers);
        }

        [HttpGet("{userId:guid}/following")]
        public async Task<ActionResult<IEnumerable<User>>> GetFollowing(Guid userId)
        {
            var followingIds = await _context.Follows
                .Where(f => f.UserFollowerId == userId)
                .Select(f => f.UserId)
                .ToListAsync();

            var following = await _context.Users
                .Where(u => followingIds.Contains(u.Id))
                .ToListAsync();

            return Ok(following);
        }

        [HttpGet("{userId:guid}/is-following/{targetId:guid}", Name = "IsFollowing")]
        public async Task<ActionResult<bool>> IsFollowing(Guid userId, Guid targetId)
        {
            var exists = await _context.Follows.FindAsync(userId, targetId);
            return Ok(exists is not null);
        }
    }
}
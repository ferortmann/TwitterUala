using ApiTwitterUala.Domain.Context;
using ApiTwitterUala.DTOs;
using ApiTwitterUala.Mappers;
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
        public async Task<IActionResult> Follow([FromBody] FollowDto followDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var targetExists = await _context.Users.AnyAsync(u => u.Id == followDto.UserId);
            if (!targetExists)
                return NotFound();

            var followerExists = await _context.Users.AnyAsync(u => u.Id == followDto.UserFollowerId);
            if (!followerExists)
                return NotFound(new { Message = "Seguidor no enconrado." });

            var existing = await _context.Follows.FindAsync(followDto.UserId, followDto.UserFollowerId);
            if (existing is not null)
                return Conflict(new { Message = "Ya se están siguiendo." });

            var entity = followDto.ToEntity();
            _context.Follows.Add(entity);
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
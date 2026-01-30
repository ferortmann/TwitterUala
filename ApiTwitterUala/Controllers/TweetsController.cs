using ApiTwitterUala.Domain.Context;
using ApiTwitterUala.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiTwitterUala.Mappers;

namespace ApiTwitterUala.Controllers
{
    [ApiController]
    [Route("api/tweets")]
    public class TweetsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TweetsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TweetDto tweetDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userExists = await _context.Users.AnyAsync(u => u.Id == tweetDto.UserId);
            if (!userExists)
                return NotFound(new { Message = "Usuario no encontrado.", UserId = tweetDto.UserId });

            var entity = tweetDto.ToEntity();
            _context.Tweets.Add(entity);
            await _context.SaveChangesAsync();

            return Created();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var t = await _context.Tweets.FindAsync(id);
            return t is null ? NotFound() : Ok(t);
        }

        [HttpGet]
        public IActionResult List()
        {
            return Ok(_context.Tweets.ToList());
        }
    }
}
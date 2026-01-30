
using ApiTwitterUala.Domain.Context;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Create(Domain.Entities.Tweet tweet)
        {
            _context.Tweets.Add(tweet);
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

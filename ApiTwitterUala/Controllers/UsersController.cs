using ApiTwitterUala.Domain.Context;
using ApiTwitterUala.Domain.Entities;
using ApiTwitterUala.Services.DTOs;
using ApiTwitterUala.Services.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiTwitterUala.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _context.Users.AsNoTracking().AnyAsync(u => u.UserName == userDto.UserName || u.Id == userDto.Id);
            if (exists)
                return Conflict(new { Message = "Usuario ya existe." });

            var entity = userDto.ToEntity();
            _context.Users.Add(entity);
            await _context.SaveChangesAsync();

            return Created();
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var users = await _context.Users.AsNoTracking().ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            return user is null ? NotFound() : Ok(user);
        }
    }
}
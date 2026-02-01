using ApiTwitterUala.Domain.Entities;
using ApiTwitterUala.Services.DTOs;

namespace ApiTwitterUala.Services.Mappers
{
    public static class TweetMapper
    {
        public static Tweet ToEntity(this TweetDto dto)
            => new()
            {
                UserId = dto.UserId,
                Content = dto.Content
            };
    }
}

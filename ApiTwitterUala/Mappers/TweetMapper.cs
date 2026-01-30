using ApiTwitterUala.Domain.Entities;
using ApiTwitterUala.DTOs;

namespace ApiTwitterUala.Mappers
{
    internal static class TweetMapper
    {
        public static Tweet ToEntity(this TweetDto dto)
            => new()
            {
                UserId = dto.UserId,
                Content = dto.Content
            };
    }
}

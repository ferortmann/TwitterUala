using ApiTwitterUala.Domain.Entities;
using ApiTwitterUala.Services.DTOs;

namespace ApiTwitterUala.Services.Mappers
{
    public static class FollowMapper
    {
        public static Follow ToEntity(this FollowDto dto)
            => new()
            {
                UserId = dto.UserId,
                UserFollowerId = dto.UserFollowerId
            };
    }
}

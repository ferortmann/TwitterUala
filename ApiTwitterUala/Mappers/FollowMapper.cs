using ApiTwitterUala.Domain.Entities;
using ApiTwitterUala.DTOs;

namespace ApiTwitterUala.Mappers
{
    internal static class FollowMapper
    {
        public static Follow ToEntity(this FollowDto dto)
            => new()
            {
                UserId = dto.UserId,
                UserFollowerId = dto.UserFollowerId
            };
    }
}

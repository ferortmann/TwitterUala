using System;
using ApiTwitterUala.Domain.Entities;
using ApiTwitterUala.Services.DTOs;

namespace ApiTwitterUala.Services.Mappers
{
    public static class UserMapper
    {
        public static User ToEntity(this UserDto dto)
            => new()
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                UserName = dto.UserName
            };
    }
}
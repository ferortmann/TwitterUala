using ApiTwitterUala.Controllers;
using ApiTwitterUala.Domain.Context;
using ApiTwitterUala.Services.DTOs;
using ApiTwitterUala.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ApiTwitterUala.Tests.Controllers
{
    public class FollowControllerTests
    {
        private static readonly Guid UserA = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid UserB = Guid.Parse("22222222-2222-2222-2222-222222222222");

        private static void EnsureUsersExist(AppDbContext context)
        {
            if (!context.Users.AnyAsync(u => u.Id == UserA).GetAwaiter().GetResult())
                context.Users.Add(new ApiTwitterUala.Domain.Entities.User { Id = UserA, UserName = "usuario1234" });

            if (!context.Users.AnyAsync(u => u.Id == UserB).GetAwaiter().GetResult())
                context.Users.Add(new ApiTwitterUala.Domain.Entities.User { Id = UserB, UserName = "usuario9876" });

            context.SaveChanges();
        }

        [Fact]
        public async Task Follow_ShouldCreateFollow_WhenValid()
        {
            using var context = TestDbContextFactory.CreateInMemoryContext();
            EnsureUsersExist(context);

            var controller = new FollowController(context);

            var dto = new FollowDto
            {
                UserId = UserA,
                UserFollowerId = UserB
            };

            ModelValidator.ValidateAndPopulateModelState(dto, controller);

            var result = await controller.Follow(dto, CancellationToken.None);

            result.Should().NotBeNull();
            var persisted = context.Follows.Find(dto.UserId, dto.UserFollowerId);
            persisted.Should().NotBeNull();
        }

        [Fact]
        public async Task Follow_ShouldReturnConflict_WhenAlreadyFollowing()
        {
            using var context = TestDbContextFactory.CreateInMemoryContext();
            EnsureUsersExist(context);

            context.Follows.Add(new ApiTwitterUala.Domain.Entities.Follow
            {
                UserId = UserA,
                UserFollowerId = UserB
            });
            context.SaveChanges();

            var controller = new FollowController(context);
            var dto = new FollowDto
            {
                UserId = UserA,
                UserFollowerId = UserB
            };

            ModelValidator.ValidateAndPopulateModelState(dto, controller);

            var result = await controller.Follow(dto, CancellationToken.None);
            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task Follow_ShouldReturnBadRequest_WhenSelfFollow()
        {
            using var context = TestDbContextFactory.CreateInMemoryContext();
            EnsureUsersExist(context);

            var controller = new FollowController(context);

            var dto = new FollowDto
            {
                UserId = UserA,
                UserFollowerId = UserA 
            };

            ModelValidator.ValidateAndPopulateModelState(dto, controller);

            var result = await controller.Follow(dto, CancellationToken.None);
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
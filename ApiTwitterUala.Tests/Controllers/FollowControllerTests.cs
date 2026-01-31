using ApiTwitterUala.Controllers;
using ApiTwitterUala.DTOs;
using ApiTwitterUala.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ApiTwitterUala.Tests.Controllers
{
    public class FollowControllerTests
    {
        [Fact]
        public async Task Follow_ShouldCreateFollow_WhenValid()
        {
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var controller = new FollowController(context);

            var dto = new FollowDto
            {
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                UserFollowerId = Guid.Parse("22222222-2222-2222-2222-222222222222")
            };

            ModelValidator.ValidateAndPopulateModelState(dto, controller);

            var result = await controller.Follow(dto);

            var persisted = context.Follows.Find(dto.UserId, dto.UserFollowerId);
            persisted.Should().NotBeNull();
        }

        [Fact]
        public async Task Follow_ShouldReturnConflict_WhenAlreadyFollowing()
        {
            using var context = TestDbContextFactory.CreateInMemoryContext();
            context.Follows.Add(new ApiTwitterUala.Domain.Entities.Follow
            {
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                UserFollowerId = Guid.Parse("22222222-2222-2222-2222-222222222222")
            });
            context.SaveChanges();

            var controller = new FollowController(context);
            var dto = new FollowDto
            {
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                UserFollowerId = Guid.Parse("22222222-2222-2222-2222-222222222222")
            };

            ModelValidator.ValidateAndPopulateModelState(dto, controller);

            var result = await controller.Follow(dto);
            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task Follow_ShouldReturnBadRequest_WhenSelfFollow()
        {
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var controller = new FollowController(context);

            var dto = new FollowDto
            {
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                UserFollowerId = Guid.Parse("11111111-1111-1111-1111-111111111111") // same
            };

            ModelValidator.ValidateAndPopulateModelState(dto, controller);

            var result = await controller.Follow(dto);
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
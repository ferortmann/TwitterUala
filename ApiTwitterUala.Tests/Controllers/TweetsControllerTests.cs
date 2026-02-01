using ApiTwitterUala.Controllers;
using ApiTwitterUala.Services.DTOs;
using ApiTwitterUala.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ApiTwitterUala.Domain.Entities;

namespace ApiTwitterUala.Tests.Controllers
{
    public class TweetsControllerTests
    {
        [Fact]
        public async Task Create_ShouldPersistTweet_WhenDtoIsValidAndUserExists()
        {
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var controller = new TweetsController(context, null, new NoOpBackgroundTaskQueue());

            var dto = new TweetDto
            {
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Content = "Tweet de test"
            };

            ModelValidator.ValidateAndPopulateModelState(dto, controller);

            var result = await controller.Create(dto);

            controller.ModelState.IsValid.Should().BeTrue();
            var persisted = context.Tweets.SingleOrDefault(t => t.Content == dto.Content && t.UserId == dto.UserId);
            persisted.Should().NotBeNull();
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenUserDoesNotExist()
        {
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var controller = new TweetsController(context, null, new NoOpBackgroundTaskQueue());

            var dto = new TweetDto
            {
                UserId = Guid.NewGuid(),
                Content = "Hola Tweet de prueba!"
            };

            ModelValidator.ValidateAndPopulateModelState(dto, controller);

            var result = await controller.Create(dto);
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenContentContainsForbiddenCharacter()
        {
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var controller = new TweetsController(context, null, new NoOpBackgroundTaskQueue());

            var dto = new TweetDto
            {
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Content = "Contenido inválido *"
            };

            ModelValidator.ValidateAndPopulateModelState(dto, controller);

            var result = await controller.Create(dto);

            controller.ModelState.IsValid.Should().BeFalse();
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Timeline_ShouldReturnTweetsForFollower()
        {
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var controller = new TweetsController(context, null, new NoOpBackgroundTaskQueue());

            var authorId = Guid.Parse("11111111-1111-1111-1111-111111111111"); 
            var followerId = Guid.Parse("22222222-2222-2222-2222-222222222222"); 

            var now = DateTime.UtcNow;
            var t1 = new Tweet
            {
                Id = Guid.NewGuid(),
                UserId = authorId,
                Content = "Tweet A",
                CreatedAt = now
            };
            var t2 = new Tweet
            {
                Id = Guid.NewGuid(),
                UserId = authorId,
                Content = "Tweet B",
                CreatedAt = now.AddMinutes(-1)
            };

            context.Tweets.AddRange(t1, t2);
            context.Follows.Add(new Follow { UserId = authorId, UserFollowerId = followerId });
            context.SaveChanges();

            var result = await controller.TimeLine(followerId);

            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok!.Value.Should().BeAssignableTo<System.Collections.IEnumerable>();

            var tweets = (ok.Value as System.Collections.Generic.IEnumerable<TweetViewDto>)?.ToList() ?? [];
            tweets.Should().HaveCount(2);
            tweets[0].Content.Should().Be("Tweet A");
            tweets[1].Content.Should().Be("Tweet B");
        }
    }
}
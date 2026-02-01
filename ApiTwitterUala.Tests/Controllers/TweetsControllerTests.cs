using ApiTwitterUala.Controllers;
using ApiTwitterUala.DTOs;
using ApiTwitterUala.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

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
    }
}
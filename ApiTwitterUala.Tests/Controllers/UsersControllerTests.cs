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
using System.Collections.Generic;

namespace ApiTwitterUala.Tests.Controllers
{
    public class UsersControllerTests
    {
        [Fact]
        public async Task Create_ShouldCreateUser_WhenDtoIsValid()
        {
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var controller = new UsersController(context);

            var dto = new UserDto
            {
                UserName = "new_user_1"
            };

            ModelValidator.ValidateAndPopulateModelState(dto, controller);

            var result = await controller.Create(dto);

            result.Should().BeOfType<CreatedResult>();
            var created = context.Users.SingleOrDefault(u => u.UserName == dto.UserName);
            created.Should().NotBeNull();
        }

        [Fact]
        public async Task Create_ShouldReturnConflict_WhenUserNameAlreadyExists()
        {
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var controller = new UsersController(context);

            var dto = new UserDto
            {
                UserName = "alice"
            };

            ModelValidator.ValidateAndPopulateModelState(dto, controller);

            var result = await controller.Create(dto);

            result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenUserNameContainsInvalidCharacters()
        {
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var controller = new UsersController(context);

            var dto = new UserDto
            {
                UserName = "invalid*name!" // contains '*' and '!' which are not allowed by the regex
            };

            ModelValidator.ValidateAndPopulateModelState(dto, controller);

            var result = await controller.Create(dto);

            controller.ModelState.IsValid.Should().BeFalse();
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task List_ShouldReturnAllUsers()
        {
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var controller = new UsersController(context);

            var result = await controller.List();

            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok!.Value.Should().BeAssignableTo<IEnumerable<User>>();

            var users = ((IEnumerable<User>?)ok.Value ?? Enumerable.Empty<User>()).ToList();
            users.Count.Should().BeGreaterOrEqualTo(2);
        }

        [Fact]
        public async Task GetById_ShouldReturnUser_WhenExists()
        {
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var controller = new UsersController(context);

            var existingId = Guid.Parse("11111111-1111-1111-1111-111111111111"); 
            var result = await controller.GetById(existingId);

            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok!.Value.Should().BeAssignableTo<User>();
            var user = (User)ok.Value!;
            user.Id.Should().Be(existingId);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenDoesNotExist()
        {
            using var context = TestDbContextFactory.CreateInMemoryContext();
            var controller = new UsersController(context);

            var missingId = Guid.NewGuid();
            var result = await controller.GetById(missingId);

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
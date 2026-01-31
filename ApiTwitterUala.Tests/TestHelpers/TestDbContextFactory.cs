using ApiTwitterUala.Domain.Context;
using ApiTwitterUala.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace ApiTwitterUala.Tests.TestHelpers
{
    internal static class TestDbContextFactory
    {
        public static AppDbContext CreateInMemoryContext(string dbName = null)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();

            // Seed two users
            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), UserName = "alice" },
                    new User { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), UserName = "bob" }
                );
                context.SaveChanges();
            }

            return context;
        }
    }
}
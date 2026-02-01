using ApiTwitterUala.Domain.Context;
using ApiTwitterUala.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ApiTwitterUala.Services.Cache.Services.Interfaces;
using ApiTwitterUala.Services.Cache.Services.Impl;
using ApiTwitterUala.Services.BackgroundTasks;
using ApiTwitterUala.Cache.Services.Interfaces;
using ApiTwitterUala.Cache.Services.Impl;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TwitterUala"));

builder.Services.AddDistributedMemoryCache();

builder.Services.AddScoped<IFollowCacheService, InMemoryFollowCacheService>();
builder.Services.AddScoped<ITweetCacheService, InMemoryTweetCacheService>();
builder.Services.AddScoped<ITweetCacheUpdaterService, InMemoryTweetCacheUpdaterService>();

builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<QueuedHostedService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var scopeProvider = scope.ServiceProvider;
    var context = scopeProvider.GetRequiredService<AppDbContext>();

    context.Database.EnsureCreated();

    if (!context.Users.Any())
    {
        var user1 = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            UserName = "usuario1234"
        };

        var user2 = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            UserName = "usuario9876"
        };

        var user3 = new User
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            UserName = "usuario_alpha"
        };

        var user4 = new User
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            UserName = "usuario_beta"
        };

        var user5 = new User
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            UserName = "usuario_gamma"
        };

        context.Users.AddRange(user1, user2, user3, user4, user5);
        context.SaveChanges();

        // Add 5 sample tweets for user1 if none exist
        if (!context.Tweets.Any(t => t.UserId == user1.Id))
        {
            var now = DateTime.UtcNow;
            var tweets = Enumerable.Range(1, 5)
                .Select(i => new Tweet
                {
                    Id = Guid.NewGuid(),
                    UserId = user1.Id,
                    Content = $"Tweet de prueba {i}",
                    CreatedAt = now.AddSeconds(-i)
                })
                .ToList();

            context.Tweets.AddRange(tweets);
            context.SaveChanges();
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

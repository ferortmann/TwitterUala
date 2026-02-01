using ApiTwitterUala.Cache.Services;
using ApiTwitterUala.Domain.Context;
using ApiTwitterUala.Domain.Entities;
using ApiTwitterUala.BackgroundTasks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TwitterUala"));

// Use in-memory distributed cache (remove Redis dependency)
builder.Services.AddDistributedMemoryCache();

// Register in-memory cache services (replace Redis implementations)
builder.Services.AddScoped<IFollowCacheService, InMemoryFollowCacheService>();
builder.Services.AddScoped<ITweetCacheService, InMemoryTweetCacheService>();

// Background task queue and hosted worker
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

        context.Users.AddRange(user1, user2);
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

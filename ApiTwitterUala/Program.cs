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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Disable HTTPS redirection when running inside a container to avoid redirect loops
var runningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
if (!runningInContainer)
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

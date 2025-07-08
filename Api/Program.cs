using Api.Extensions;
using Api.Middleware;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Caching;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add Serilog
//LoggingService.ConfigureLogging();
//builder.Host.UseSerilog();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Add conditional auth
builder.Services.AddConditionalAuthentication(builder.Configuration, builder.Environment);

// Add HttpClient for AuthService
builder.Services.AddHttpClient<IAuthService, AuthService>();

// Register AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// Add SQLite DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add Memory Cache
builder.Services.AddMemoryCache();

// Register services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add global workspace cache
builder.Services.AddCachingServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseCors("AllowSpecificOrigin");


app.UseAplicationInterceptor();
app.UseGlobalExceptionHandling();

//app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
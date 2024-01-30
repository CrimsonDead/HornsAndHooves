using API.Hubs;
using API.Infrastructure.Authorization;
using DBL;
using DBL.DTOs;
using DBL.Identity;
using DBL.Interfaces;
using DBL.Models;
using DBL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var AllowSpecificOrigins = "allowedSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: AllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin();
        });
});

builder.Services.AddControllers();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddDbContext<ApplicationContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddIdentity<User, Role>(o =>
{
    o.Password.RequireDigit = true;
    o.Password.RequireNonAlphanumeric = false;
    o.Stores.MaxLengthForKeys = 128;
    o.SignIn.RequireConfirmedAccount = true;
}).AddDefaultTokenProviders()
  .AddEntityFrameworkStores<ApplicationContext>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization().AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationResultHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.TryAddScoped<IRepository<UserDTO, string>, UserRepository>();
builder.Services.TryAddScoped<IRepository<ChatDTO, string>, ChatRepository>();
builder.Services.TryAddScoped<IRepository<MessageDTO, string>, MessageRepository>();

builder.Services.TryAddScoped<AuthorizationManager<UserDTO>>();

builder.Services.AddTransient<IAuthorizationPolicyProvider, JWTAuthorizationPolicyProvider>();
builder.Services.AddTransient<IAuthorizationHandler, JwtAuthorizationHandler>();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseCors(AllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatsocket");

app.Run();

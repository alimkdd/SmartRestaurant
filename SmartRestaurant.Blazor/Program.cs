using Blazored.LocalStorage;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartRestaurant.Application.Common;
using SmartRestaurant.Application.Interfaces;
using SmartRestaurant.Application.Interfaces.Abstractions.AI;
using SmartRestaurant.Application.Interfaces.Abstractions.Caching;
using SmartRestaurant.Application.Services.AI;
using SmartRestaurant.Application.Services.Authentication;
using SmartRestaurant.Application.Services.Caching;
using SmartRestaurant.Application.Services.Inventory;
using SmartRestaurant.Application.Services.Menu;
using SmartRestaurant.Application.Services.Orders;
using SmartRestaurant.Application.Validations;
using SmartRestaurant.Infrastructure.Context;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Application Services
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IChatbotService, ChatbotService>();

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestModelValidator>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    return ConnectionMultiplexer.Connect(builder.Configuration.GetValue<string>("Redis:Connection")!);
});

builder.Services.AddScoped<ICacheService, CacheService>();

// AI
builder.Services.AddHttpClient<IChatbotService, ChatbotService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AI:BaseUrl"]!);
});

// Razor Pages + Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Staff", policy => policy.RequireRole("Admin", "Staff"));
    options.AddPolicy("Customer", policy => policy.RequireRole("Admin", "Staff", "Customer"));
});

var app = builder.Build();

// Middleware order is important!
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
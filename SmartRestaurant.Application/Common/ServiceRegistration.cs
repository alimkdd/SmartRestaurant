using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartRestaurant.Application.Dtos.Requests;
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
using StackExchange.Redis;

namespace SmartRestaurant.Application.Common;

public static class ServiceRegistration
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration)
    {
        services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IInventoryService, InventoryService>();

        services.AddSingleton<IValidator<LoginRequestModel>, LoginRequestModelValidator>();
        services.AddSingleton<IValidator<RegisterRequestModel>, RegisterRequestModelValidator>();
        services.AddSingleton<IValidator<RefreshTokenRequestModel>, RefreshTokenRequestModelValidator>();
        services.AddFluentValidationAutoValidation();


        // Redis
        services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(configuration["Redis:Connection"]));
        services.AddScoped<ICacheService, CacheService>();

        // AI
        services.AddHttpClient<IChatbotService, ChatbotService>(client =>
        {
            client.BaseAddress = new Uri(configuration["AI:BaseUrl"]);
        });

        return services;
    }
}
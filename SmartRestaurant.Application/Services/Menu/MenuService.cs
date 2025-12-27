using SmartRestaurant.Application.Interfaces;
using SmartRestaurant.Application.Services.Common;
using SmartRestaurant.Domain.Models;
using SmartRestaurant.Infrastructure.Context;

namespace SmartRestaurant.Application.Services.Menu;

public class MenuService : ServiceBase<MenuItem>, IMenuService
{
    public MenuService(AppDbContext context) : base(context)
    {
    }
}
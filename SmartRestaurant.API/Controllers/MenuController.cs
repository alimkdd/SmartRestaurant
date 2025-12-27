using SmartRestaurant.Application.Interfaces;
using SmartRestaurant.Domain.Models;

namespace SmartRestaurant.API.Controllers;

public class MenuController : ServiceBaseController<MenuItem>
{
    public MenuController(IMenuService service) : base(service)
    {
    }
}
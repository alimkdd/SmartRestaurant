using SmartRestaurant.Domain.Models;
using SmartRestaurant.Application.Interfaces;

namespace SmartRestaurant.API.Controllers;

public class InventoryController : ServiceBaseController<InventoryItem>
{
    public InventoryController(IInventoryService service) : base(service)
    {
    }
}
using SmartRestaurant.Application.Interfaces.Common;
using SmartRestaurant.Domain.Models;

namespace SmartRestaurant.Application.Interfaces;

public interface IInventoryService : IServiceBase<InventoryItem>
{
    Task DeductIngredients(List<OrderItem> orderItems, CancellationToken ct = default);
    Task<List<InventoryItem>> GetLowStockItems();
}
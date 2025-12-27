using SmartRestaurant.Application.Interfaces.Common;
using SmartRestaurant.Domain.Models;

namespace SmartRestaurant.Application.Interfaces;

public interface IOrderService : IServiceBase<Order>
{
    Task<List<Order>> GetOrdersByCustomer(Guid customerId);
    Task<string> RecommendMenu(Guid customerId, CancellationToken ct = default);
}
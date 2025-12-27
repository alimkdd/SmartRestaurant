using SmartRestaurant.Domain.Models.Common;

namespace SmartRestaurant.Domain.Models;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
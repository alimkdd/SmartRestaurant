using SmartRestaurant.Domain.Enums;
using SmartRestaurant.Domain.Models.Common;

namespace SmartRestaurant.Domain.Models;

public class Order : BaseEntity
{
    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public OrderStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<OrderItem> Items { get; set; } = [];
}
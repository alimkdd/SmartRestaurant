using SmartRestaurant.Domain.Models.Common;

namespace SmartRestaurant.Domain.Models;

public class InventoryItem : BaseEntity
{
    public string Name { get; set; } = default!;
    public string Unit { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal MinimumThreshold { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
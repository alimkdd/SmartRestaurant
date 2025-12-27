using SmartRestaurant.Domain.Models.Common;

namespace SmartRestaurant.Domain.Models;

public class Ingredient : BaseEntity
{
    public string Name { get; set; } = default!;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = default!;

    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = default!;
}
using SmartRestaurant.Domain.Models.Common;

namespace SmartRestaurant.Domain.Models;

public class MenuItem : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Image { get; set; }
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public ICollection<Ingredient> Ingredients { get; set; }
}
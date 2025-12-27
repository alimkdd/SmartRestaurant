using SmartRestaurant.Domain.Enums;
using SmartRestaurant.Domain.Models.Common;

namespace SmartRestaurant.Domain.Models;

public class User : BaseEntity
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

}

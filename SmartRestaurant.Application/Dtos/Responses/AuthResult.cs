namespace SmartRestaurant.Application.Dtos.Responses;

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? Role { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }
    public double? LockoutRemainingSeconds { get; set; }
}

namespace SmartRestaurant.Application.Dtos.Requests;

public class RegisterRequestModel
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;
}
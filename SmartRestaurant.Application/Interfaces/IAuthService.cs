using SmartRestaurant.Application.Dtos.Requests;
using SmartRestaurant.Application.Dtos.Responses;

namespace SmartRestaurant.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResult> Login(LoginRequestModel request);

    Task<AuthResult> Register(RegisterRequestModel request);

    Task<bool> ValidateToken(string token);

    Task<string> RefreshToken(RefreshTokenRequestModel request);

    Task Logout();
}
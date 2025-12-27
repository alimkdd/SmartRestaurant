using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using SmartRestaurant.Application.Dtos.Requests;
using SmartRestaurant.Application.Dtos.Responses;
using SmartRestaurant.Application.Services.Authentication;

namespace SmartRestaurant.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<AuthResult> Register([FromBody] RegisterRequestModel request)
        => await _authService.Register(request);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<AuthResult> Login([FromBody] LoginRequestModel request)
        => await _authService.Login(request);


    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<string> Refresh([FromBody] RefreshTokenRequestModel request)
        => await _authService.RefreshToken(request);


    [HttpPost("validate")]
    [Authorize]
    public async Task<IActionResult> ValidateToken()
    {
        var token = Request.Headers.Authorization
            .ToString()
            .Replace("Bearer ", "");

        var isValid = await _authService.ValidateToken(token);
        return Ok(new { valid = isValid });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task Logout() => await _authService.Logout();
}
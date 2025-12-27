using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartRestaurant.Application.Dtos.Requests;
using SmartRestaurant.Application.Dtos.Responses;
using SmartRestaurant.Application.Interfaces;
using SmartRestaurant.Application.Interfaces.Abstractions.Caching;
using SmartRestaurant.Domain.Enums;
using SmartRestaurant.Domain.Models;
using SmartRestaurant.Infrastructure.Context;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Text;

namespace SmartRestaurant.Application.Services.Authentication;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ICacheService _cacheService;
    private readonly AppDbContext _dbContext;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _jwtExpiryMinutes;
    private readonly int _loginAttemptLimit;
    private readonly int _lockoutDurationMinutes;

    public AuthService(IConfiguration configuration, ICacheService cacheService, AppDbContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
        _cacheService = cacheService;

        _jwtSecret = _configuration["Jwt:Secret"];
        _jwtIssuer = _configuration["Jwt:Issuer"];
        _jwtAudience = _configuration["Jwt:Audience"];
        _jwtExpiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes");

        _loginAttemptLimit = _configuration.GetValue<int>("Auth:LoginAttemptLimit");
        _lockoutDurationMinutes = _configuration.GetValue<int>("Auth:LockoutDurationMinutes");
    }

    public async Task<AuthResult> Register(RegisterRequestModel request)
    {
        // Check if the user already exists
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (existingUser != null)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Email already registered"
            };
        }

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLower(),
            PasswordHash = HashPassword(request.Password),
            Role = Enum.TryParse<UserRole>(request.Role, true, out var parsedRole) ? parsedRole : UserRole.Customer,
            RefreshToken = GenerateRefreshToken(),
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7) // e.g., 7 days expiry
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Store tokens in local storage
        var token = GenerateJwtToken(user);
        await _cacheService.SetAsync("authToken", token, TimeSpan.FromMinutes(_jwtExpiryMinutes));
        await _cacheService.SetAsync("refreshToken", user.RefreshToken, TimeSpan.FromDays(7));
        await _cacheService.SetAsync("userEmail", user.Email, TimeSpan.FromDays(7));
        await _cacheService.SetAsync("userRole", user.Role.ToString(), TimeSpan.FromDays(7));


        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtExpiryMinutes);

        return new AuthResult
        {
            Success = true,
            Token = token,
            RefreshToken = user.RefreshToken,
            Role = user.Role.ToString(),
            UserId = user.Id.ToString(),
            Email = user.Email,
            ExpiresAt = expiresAt
        };
    }

    public async Task<AuthResult> Login(LoginRequestModel model)
    {
        var key = $"login_attempts:{model.Email.ToLower()}";

        // Check failed attempts in Redis
        var attempts = await _cacheService.GetAsync<int>(key);
        if (attempts >= _loginAttemptLimit)
        {
            var ttl = await _cacheService.GetLockoutTTL(key);
            return new AuthResult
            {
                Success = false,
                ErrorMessage = $"Too many failed attempts. Try again in {ttl?.Minutes} minutes {ttl?.Seconds} seconds.",
                LockoutRemainingSeconds = ttl?.TotalSeconds ?? _lockoutDurationMinutes * 60
            };
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

        if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
        {
            // Increment failed attempts
            await _cacheService.SetAsync(key, attempts + 1, TimeSpan.FromMinutes(_lockoutDurationMinutes));

            return new AuthResult
            {
                Success = false,
                ErrorMessage = "Invalid email or password."
            };
        }

        // Successful login => reset attempts
        await _cacheService.RemoveAsync(key);

        // Generate JWT and refresh token
        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _dbContext.SaveChangesAsync();

        await _cacheService.SetAsync("authToken", token, TimeSpan.FromMinutes(_jwtExpiryMinutes));
        await _cacheService.SetAsync("refreshToken", refreshToken, TimeSpan.FromDays(7));
        await _cacheService.SetAsync("userEmail", user.Email, TimeSpan.FromDays(7));
        await _cacheService.SetAsync("userRole", user.Role.ToString(), TimeSpan.FromDays(7));

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtExpiryMinutes);

        return new AuthResult
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken,
            Role = user.Role.ToString(),
            UserId = user.Id.ToString(),
            Email = user.Email,
            ExpiresAt = expiresAt
        };
    }
    public async Task<bool> ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return false;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = true,
                ValidAudience = _jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> RefreshToken(RefreshTokenRequestModel request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new SecurityException("Invalid or expired refresh token");

        var newToken = GenerateJwtToken(user);

        // Optionally issue a new refresh token
        var newRefreshToken = GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _dbContext.SaveChangesAsync();

        await _cacheService.SetAsync($"authToken", newToken, TimeSpan.FromMinutes(_jwtExpiryMinutes));
        await _cacheService.SetAsync($"refreshToken", newRefreshToken, TimeSpan.FromDays(7));

        return newToken;
    }

    public async Task Logout()
    {
        await _cacheService.RemoveAsync("authToken");
        await _cacheService.RemoveAsync("refreshToken");
        await _cacheService.RemoveAsync("userEmail");
        await _cacheService.RemoveAsync("userRole");
    }
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtExpiryMinutes),
            Issuer = _jwtIssuer,
            Audience = _jwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    private string GenerateRefreshToken() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    private static string HashPassword(string password) => Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
    private static bool VerifyPassword(string password, string hash) => Convert.ToBase64String(Encoding.UTF8.GetBytes(password)) == hash;
}
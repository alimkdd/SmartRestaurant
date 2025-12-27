using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartRestaurant.Application.Interfaces.Abstractions.Caching;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartRestaurant.Application.Services.Authentication;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IConfiguration _configuration;
    private readonly ICacheService _cacheService;
    private readonly string _jwtSecret;

    public CustomAuthStateProvider(ICacheService cacheService, IConfiguration configuration)
    {
        _cacheService = cacheService;
        _configuration = configuration;
        _jwtSecret = _configuration["Jwt:Secret"];
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _cacheService.GetAsync<string>("authToken");

            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Validate and parse token
            var claims = ParseClaimsFromJwt(token);

            if (claims == null || !claims.Any())
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Check if token is expired
            var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
            if (expClaim != null)
            {
                var exp = long.Parse(expClaim.Value);
                var expDate = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;

                if (expDate < DateTime.UtcNow)
                {
                    // Token expired, clear storage
                    await _cacheService.RemoveAsync("authToken");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public async Task Logout()
    {
        await _cacheService.RemoveAsync("authToken");
        await _cacheService.RemoveAsync("refreshToken");
        await _cacheService.RemoveAsync("userEmail");
        await _cacheService.RemoveAsync("userRole");
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private IEnumerable<Claim>? ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            tokenHandler.ValidateToken(jwt, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            var claims = jwtToken.Claims.ToList();

            // Map "role" claims to ClaimTypes.Role
            var roleClaims = claims
                .Where(c => c.Type == "role" || c.Type == "roles")
                .Select(c => new Claim(ClaimTypes.Role, c.Value))
                .ToList();

            claims.AddRange(roleClaims);

            return claims;
        }
        catch
        {
            return null;
        }
    }

}
using Microsoft.Extensions.Configuration;
using SmartRestaurant.Application.Interfaces.Abstractions.Caching;
using StackExchange.Redis;
using System.Text.Json;

namespace SmartRestaurant.Application.Services.Caching;

public class CacheService : ICacheService
{
    private readonly IDatabase _db;
    private readonly IConfiguration _configuration;
    private int LoginAttemptLimit = 0;
    private int LockoutDurationMinutes = 0;
    public CacheService(IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _configuration = configuration;
        _db = redis.GetDatabase();

        LoginAttemptLimit = _configuration.GetValue<int>("Auth:LoginAttemptLimit");
        LockoutDurationMinutes = _configuration.GetValue<int>("Auth:LockoutDurationMinutes");
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
    }

    public async Task SetAsync<T>(string key, T value, Expiration expiry)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiry);
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }

    public async Task<TimeSpan?> GetLockoutTTL(string key)
    {
        var ttl = await _db.KeyTimeToLiveAsync(key);
        return ttl;
    }

    public int GetLockoutDuration() => LockoutDurationMinutes;

    public async Task<int> IncrementLoginAttempts(string email)
    {
        var key = $"login_attempt_{email.ToLower()}";
        var attempts = await GetAsync<int?>(key) ?? 0;

        attempts++;
        var remaining = LoginAttemptLimit - attempts;

        if (remaining > 0)
        {
            await SetAsync(key, attempts, TimeSpan.FromMinutes(LockoutDurationMinutes));
        }

        return remaining;
    }

    public async Task SetLockout(string email, int lockoutMinutes)
    {
        var key = $"login_lockout_{email.ToLower()}";
        await SetAsync(key, true, TimeSpan.FromMinutes(lockoutMinutes));
    }

    public async Task ClearLoginAttempts(string email)
    {
        var attemptKey = $"login_attempt_{email.ToLower()}";
        var lockoutKey = $"login_lockout_{email.ToLower()}";

        await RemoveAsync(attemptKey);
        await RemoveAsync(lockoutKey);
    }
}
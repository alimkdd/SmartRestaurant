using StackExchange.Redis;

namespace SmartRestaurant.Application.Interfaces.Abstractions.Caching;

public interface ICacheService
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, Expiration expiry);
    Task RemoveAsync(string key);
    Task<TimeSpan?> GetLockoutTTL(string key);
    int GetLockoutDuration();
    Task<int> IncrementLoginAttempts(string email);
    Task SetLockout(string email, int lockoutMinutes);
    Task ClearLoginAttempts(string email);
}
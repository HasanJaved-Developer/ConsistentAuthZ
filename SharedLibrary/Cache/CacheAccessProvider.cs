using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace SharedLibrary.Cache
{
    public sealed class CacheAccessProvider : ICacheAccessProvider
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _http;        

        public CacheAccessProvider(IMemoryCache cache, IHttpContextAccessor http)
        {
            _cache = cache;
            _http = http;
        }

        public Task<string?> GetAccessTokenAsync(CancellationToken ct = default)
        {
            var user = _http.HttpContext?.User;
            var uid =
                user?.FindFirst("sub")?.Value
             ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(uid)) return Task.FromResult<string?>(null);

            var key = $"token:{uid}";
            _cache.TryGetValue(key, out string? token);
            return Task.FromResult(token);
        }

        public Task<string?> GetUserPermissionsAsync(CancellationToken ct = default)
        {
            var user = _http.HttpContext?.User;
            var uid =
                user?.FindFirst("sub")?.Value
             ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(uid)) return Task.FromResult<string?>(null);

            var key = $"perm:{uid}";
            _cache.TryGetValue(key, out string? permissions);
            return Task.FromResult(permissions);
        }

        // Optional helper method to set the token into cache
        public void SetAccessToken(string token, int userId, DateTime expiresAtUtc)
        {            
            var ttl = ToTtl(expiresAtUtc);            
            _cache.Set($"token:{userId}", token, ttl);
        }

        public void SetUserPermissions(string permissions, int userId, DateTime expiresAtUtc)
        {
            var ttl = ToTtl(expiresAtUtc);
            _cache.Set($"perm:{userId}", permissions, ttl);
        }

        public Task RemoveAsync(string userId, CancellationToken ct = default)
        {   
            _cache.Remove($"token:{userId}");
            return Task.CompletedTask;
        }

        public static TimeSpan ToTtl(DateTime expiresAtUtc, TimeSpan? safety = null)
        {
            // ensure it's treated as UTC
            if (expiresAtUtc.Kind != DateTimeKind.Utc)
                expiresAtUtc = DateTime.SpecifyKind(expiresAtUtc, DateTimeKind.Utc);

            var ttl = expiresAtUtc - DateTime.UtcNow;

            // subtract a small safety margin to avoid edge expiries
            ttl -= safety ?? TimeSpan.FromSeconds(15);

            return ttl > TimeSpan.Zero ? ttl : TimeSpan.Zero;
        }
    }
}

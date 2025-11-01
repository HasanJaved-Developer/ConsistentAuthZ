using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;

namespace SharedLibrary.Cache
{
    public sealed class CacheAccessProvider : ICacheAccessProvider
    {
        private readonly IDistributedCache _cache;
        private readonly IHttpContextAccessor _http;        

        public CacheAccessProvider(IDistributedCache cache, IHttpContextAccessor http)
        {
            _cache = cache;
            _http = http;
        }

        public async Task<string?> GetAccessTokenAsync(CancellationToken ct = default)
        {
            var user = _http.HttpContext?.User;
            var uid =
                user?.FindFirst("sub")?.Value
             ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(uid)) return null;

            var key = $"auth:token:{uid}";
            var token = await _cache.GetStringAsync(key, ct);
            return token;
            
        }

        public async Task<string?> GetUserPermissionsAsync(CancellationToken ct = default)
        {
            var user = _http.HttpContext?.User;
            var uid =
                user?.FindFirst("sub")?.Value
             ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(uid)) return null;

            var key = $"auth:permissions:{uid}";
            var permissions = await _cache.GetStringAsync(key, ct);
            return permissions;
        }

        // Optional helper method to set the token into cache
        public async Task SetAccessToken(string token, int userId, DateTime expiresAtUtc, CancellationToken ct = default)
        {
            var ttl = ToTtl(expiresAtUtc);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            // IMPORTANT: Do not manually prefix here if using InstanceName in startup.
            var key = $"auth:token:{userId}";

            await _cache.SetStringAsync(key, token, options, ct);
        }

        public async Task SetUserPermissions(string permissions, int userId, DateTime expiresAtUtc, CancellationToken ct = default)
        {
            var ttl = ToTtl(expiresAtUtc);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            // IMPORTANT: Do not manually prefix here if using InstanceName in startup.
            var key = $"auth:permissions:{userId}";

            await _cache.SetStringAsync(key, permissions, options, ct);
        }

        public Task RemoveAsync(string userId, CancellationToken ct = default)
        {   
            _cache.Remove($"auth:token:{userId}");
            _cache.Remove($"auth:permissions:{userId}");
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

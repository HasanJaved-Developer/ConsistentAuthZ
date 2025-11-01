using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Cache
{
    public interface ICacheAccessProvider
    {
        Task<string?> GetAccessTokenAsync(CancellationToken ct = default);
        Task<string?> GetUserPermissionsAsync(CancellationToken ct = default);        
        Task SetAccessToken(string token, int userId, DateTime expiresAtUtc, CancellationToken ct = default);
        Task SetUserPermissions(string permissions, int userId, DateTime expiresAtUtc, CancellationToken ct = default);
        public Task RemoveAsync(string userId, CancellationToken ct = default);
    }
}

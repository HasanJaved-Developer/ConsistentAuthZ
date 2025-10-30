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
        void SetAccessToken(string token, int userId, DateTime expiresAtUtc);
        void SetUserPermissions(string permissions, int userId, DateTime expiresAtUtc);
        public Task RemoveAsync(string userId, CancellationToken ct = default);
    }
}

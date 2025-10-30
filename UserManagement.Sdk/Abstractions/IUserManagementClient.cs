using UserManagement.Contracts.Auth;
using UserManagement.Contracts.DTO;

namespace UserManagement.Sdk.Abstractions
{
    public interface IUserManagementClient
    {
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    }
}

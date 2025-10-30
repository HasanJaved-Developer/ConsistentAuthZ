using UserManagement.Contracts.DTO;

namespace UserManagement.Contracts.Auth
{    
    public record AuthResponse(
      int UserId,
      string UserName,
      string Token,
      DateTime ExpiresAtUtc,
      List<CategoryDto> Categories
  );
}

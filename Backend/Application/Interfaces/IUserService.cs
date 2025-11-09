using Backend.Application.DTOs.Users;

namespace Backend.Application.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(string userId);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<List<UserDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<UserDto> UpdateAsync(string userId, UpdateUserRequest request);
    Task<bool> DeleteAsync(string userId);
    Task<bool> ActivateAsync(string userId);
    Task<bool> DeactivateAsync(string userId);
    Task<bool> AssignRoleAsync(string userId, string roleName);
    Task<bool> RemoveRoleAsync(string userId, string roleName);
    Task<List<string>> GetUserRolesAsync(string userId);
    Task<List<string>> GetUserPermissionsAsync(string userId);
    Task<bool> HasPermissionAsync(string userId, string permission);
}

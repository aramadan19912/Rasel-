using OutlookInboxManagement.DTOs.Admin;

namespace OutlookInboxManagement.Services.Admin;

public interface IUserService
{
    Task<UserSearchResponse> SearchUsersAsync(UserSearchRequest request);
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<UserDto> CreateUserAsync(CreateUserRequest request, string createdBy);
    Task<UserDto> UpdateUserAsync(string userId, UpdateUserRequest request, string updatedBy);
    Task DeleteUserAsync(string userId);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request, string resetBy);
    Task<bool> LockUserAsync(string userId, DateTimeOffset? lockoutEnd);
    Task<bool> UnlockUserAsync(string userId);
    Task<List<string>> GetUserRolesAsync(string userId);
    Task<bool> AddToRolesAsync(string userId, List<string> roles);
    Task<bool> RemoveFromRolesAsync(string userId, List<string> roles);
}

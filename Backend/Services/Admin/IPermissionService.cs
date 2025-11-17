using OutlookInboxManagement.DTOs.Admin;

namespace OutlookInboxManagement.Services.Admin;

public interface IPermissionService
{
    Task<List<PermissionDto>> GetAllPermissionsAsync();
    Task<List<PermissionsByModuleDto>> GetPermissionsByModuleAsync();
    Task<PermissionDto?> GetPermissionByIdAsync(int permissionId);
    Task<PermissionDto> CreatePermissionAsync(CreatePermissionRequest request);
    Task<PermissionDto> UpdatePermissionAsync(int permissionId, UpdatePermissionRequest request);
    Task DeletePermissionAsync(int permissionId);
}

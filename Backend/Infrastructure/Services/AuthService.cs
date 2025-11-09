using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend.Application.DTOs.Auth;
using Backend.Application.Interfaces;
using Backend.Domain.Entities.Identity;
using Backend.Domain.Enums;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IJwtService _jwtService;
    private readonly ApplicationDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<Role> roleManager,
        IJwtService jwtService,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        _context = context;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid credentials");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Invalid credentials");

        return await GenerateAuthResponseAsync(user, ipAddress);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Email already registered");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = false,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        // Assign default User role
        await _userManager.AddToRoleAsync(user, SystemRole.User);

        return await GenerateAuthResponseAsync(user, string.Empty);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            throw new UnauthorizedAccessException("Invalid access token");

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("Invalid token");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("User not found");

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userId);

        if (refreshToken == null || !refreshToken.IsActive)
            throw new UnauthorizedAccessException("Invalid refresh token");

        // Revoke old token
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.ReasonRevoked = "Replaced by new token";

        await _context.SaveChangesAsync();

        return await GenerateAuthResponseAsync(user, ipAddress);
    }

    public async Task<bool> RevokeTokenAsync(string token, string ipAddress)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null || !refreshToken.IsActive)
            return false;

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.ReasonRevoked = "Revoked by user";

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        return result.Succeeded;
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return true; // Don't reveal user existence

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // Send email with token (implement email service)
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        // Implement password reset logic
        return await Task.FromResult(true);
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        // Implement email verification logic
        return await Task.FromResult(true);
    }

    public async Task LogoutAsync(string userId)
    {
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.IsActive)
            .ToListAsync();

        foreach (var token in refreshTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReasonRevoked = "Logout";
        }

        await _context.SaveChangesAsync();
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(ApplicationUser user, string ipAddress)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetUserPermissionsAsync(user.Id);

        var accessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
        var refreshTokenString = _jwtService.GenerateRefreshToken();

        var refreshToken = await _jwtService.CreateRefreshTokenAsync(user, refreshTokenString, ipAddress);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            AccessToken = accessToken,
            RefreshToken = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            Roles = roles.ToList(),
            Permissions = permissions
        };
    }

    private async Task<List<string>> GetUserPermissionsAsync(string userId)
    {
        var permissions = new HashSet<string>();

        // Get permissions from roles
        var rolePermissions = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Name))
            .ToListAsync();

        foreach (var permission in rolePermissions)
        {
            permissions.Add(permission);
        }

        // Get direct user permissions
        var userPermissions = await _context.UserPermissions
            .Where(up => up.UserId == userId && up.IsGranted)
            .Include(up => up.Permission)
            .Select(up => up.Permission.Name)
            .ToListAsync();

        foreach (var permission in userPermissions)
        {
            permissions.Add(permission);
        }

        return permissions.ToList();
    }
}

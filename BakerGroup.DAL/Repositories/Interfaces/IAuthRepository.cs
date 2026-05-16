using BakerGroup.DAL.Models;
using Microsoft.AspNetCore.Identity;

namespace BakerGroup.DAL.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<ApplicationUser?> FindByUserNameAsync(string userName);
    Task<ApplicationUser?> FindByIdAsync(string userId);
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
    Task<IdentityResult> DeleteAsync(ApplicationUser user);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
    Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token);
    Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken refreshToken);
    Task SaveRefreshTokenAsync(RefreshToken refreshToken);
    Task RevokeRefreshTokenAsync(RefreshToken refreshToken);
}


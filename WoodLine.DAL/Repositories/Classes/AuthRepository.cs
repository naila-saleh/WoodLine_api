using WoodLine.DAL.Data;
using WoodLine.DAL.Models;
using WoodLine.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WoodLine.DAL.Repositories.Classes;

public class AuthRepository : IAuthRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public AuthRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        return _userManager.FindByEmailAsync(email);
    }

    public Task<ApplicationUser?> FindByUserNameAsync(string userName)
    {
        return _userManager.FindByNameAsync(userName);
    }

    public Task<ApplicationUser?> FindByIdAsync(string userId)
    {
        return _userManager.FindByIdAsync(userId);
    }

    public Task<IList<string>> GetRolesAsync(ApplicationUser user)
    {
        return _userManager.GetRolesAsync(user);
    }

    public Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
    {
        return _userManager.CreateAsync(user, password);
    }

    public Task<IdentityResult> DeleteAsync(ApplicationUser user)
    {
        return _userManager.DeleteAsync(user);
    }

    public Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
    {
        return _userManager.CheckPasswordAsync(user, password);
    }

    public Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
    {
        return _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token)
    {
        return _userManager.ConfirmEmailAsync(user, token);
    }

    public Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
    {
        return _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
    {
        return _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
    {
        return _userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        var now = DateTime.UtcNow;

        return await _dbContext.RefreshTokens
            .Where(rt => rt.Token == token && rt.RevokedAt == null && rt.ExpiresAt > now)
            .FirstOrDefaultAsync();
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken refreshToken)
    {
        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();
        return refreshToken;
    }

    public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
    {
        _dbContext.RefreshTokens.Update(refreshToken);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken)
    {
        refreshToken.RevokedAt = DateTime.UtcNow;
        await SaveRefreshTokenAsync(refreshToken);
    }
}


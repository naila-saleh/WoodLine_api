using BakerGroup.DAL.DTOs.Requests;
using BakerGroup.DAL.DTOs.Responses;
using Microsoft.AspNetCore.Http;

namespace BakerGroup.BLL.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, HttpRequest requestHttp);
    Task<AuthResponse> ConfirmEmailAsync(ConfirmEmailRequest request);
    Task<AuthResponse> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request, HttpRequest requestHttp);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request);
    Task<AuthResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
}


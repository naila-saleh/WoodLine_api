using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using BakerGroup.BLL.Services.Interfaces;
using BakerGroup.DAL.DTOs.Requests;
using BakerGroup.DAL.DTOs.Responses;
using BakerGroup.DAL.Models;
using BakerGroup.DAL.Repositories.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace BakerGroup.BLL.Services.Classes;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IAuthRepository authRepository, IConfiguration configuration, IEmailSender emailSender, ILogger<AuthService> logger)
    {
        _authRepository = authRepository;
        _configuration = configuration;
        _emailSender = emailSender;
        _logger = logger;
    }

     public async Task<AuthResponse> RegisterAsync(RegisterRequest request, HttpRequest requestHttp)
    {
        _logger.LogInformation($"[REGISTER] Starting registration for email: {request.Email}");
        var existingByEmail = await _authRepository.FindByEmailAsync(request.Email);
        if (existingByEmail is not null)
        {
            return Fail("Email is already registered.");
        }

        var existingByUserName = await _authRepository.FindByUserNameAsync(request.UserName);
        if (existingByUserName is not null)
        {
            return Fail("User name is already taken.");
        }

        var user = request.Adapt<ApplicationUser>();
        user.EmailConfirmed = false;

        var result = await _authRepository.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return Fail(GetErrors(result));
        }
        
        _logger.LogInformation($"[REGISTER] User created. Now attempting to send confirmation email to {user.Email}");
        try
        {
            _logger.LogInformation($"[REGISTER] Calling SendConfirmationEmailAsync for {user.Email}");
            await SendConfirmationEmailAsync(user, requestHttp);
            _logger.LogInformation($"[REGISTER] SendConfirmationEmailAsync completed successfully for {user.Email}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[REGISTER-ERROR] Registration completed but confirmation email failed for {user.Email}. Error: {ex.GetType().Name} - {ex.Message}");
            return Fail("Registration completed, but we couldn't send the confirmation email right now. Please try again.");
        }

        _logger.LogInformation($"[REGISTER] Returning success response for {user.Email}");
        return new AuthResponse
        {
            Success = true,
            Message = "Registration successful! Please check your email to confirm your account.",
            User = user.Adapt<AuthUserDto>()
        };
    }

    public async Task<AuthResponse> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request, HttpRequest requestHttp)
    {
        var user = await _authRepository.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return new AuthResponse
            {
                Success = true,
                Message = "If the account exists and is not confirmed, a new confirmation email will be sent."
            };
        }

        if (user.EmailConfirmed)
        {
            return new AuthResponse
            {
                Success = true,
                Message = "Email is already confirmed. You can log in."
            };
        }

        try
        {
            await SendConfirmationEmailAsync(user, requestHttp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Resend confirmation email failed for {user.Email}. Error: {ex.GetType().Name} - {ex.Message}");
            return Fail("We couldn't send the confirmation email right now. Please try again.");
        }

        return new AuthResponse
        {
            Success = true,
            Message = "Confirmation email sent successfully. Please check your inbox.",
            User = user.Adapt<AuthUserDto>()
        };
    }

    public async Task<AuthResponse> ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        var user = await _authRepository.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Fail("User not found.");
        }

        // Token was Base64Url-encoded when the link was built — decode it back to standard Base64
        var base64Token = request.Token.Replace('-', '+').Replace('_', '/');
        var padLength = (4 - base64Token.Length % 4) % 4;
        base64Token += new string('=', padLength);

        var result = await _authRepository.ConfirmEmailAsync(user, base64Token);
        if (!result.Succeeded)
        {
            return Fail(GetErrors(result));
        }

        return new AuthResponse
        {
            Success = true,
            Message = "Email confirmed successfully! You can now login.",
            User = user.Adapt<AuthUserDto>()
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _authRepository.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Fail("Invalid email or password.");
        }

        if (!user.EmailConfirmed)
        {
            return Fail("Please confirm your email before logging in.");
        }

        var isPasswordValid = await _authRepository.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return Fail("Invalid email or password.");
        }

        return await BuildAuthResponseAsync(user, "Login successful.");
    }

    public async Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _authRepository.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return new AuthResponse
            {
                Success = true,
                Message = "If the email exists, a reset token can be generated."
            };
        }

        var token = await _authRepository.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebUtility.UrlEncode(token);

        return new AuthResponse
        {
            Success = true,
            Message = "Password reset token generated.",
            PasswordResetToken = encodedToken,
            User = user.Adapt<AuthUserDto>()
        };
    }

    public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _authRepository.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Fail("Invalid password reset request.");
        }

        var decodedToken = WebUtility.UrlDecode(request.Token);
        var result = await _authRepository.ResetPasswordAsync(user, decodedToken, request.NewPassword);
        if (!result.Succeeded)
        {
            return Fail(GetErrors(result));
        }

        return new AuthResponse
        {
            Success = true,
            Message = "Password has been reset successfully.",
            User = user.Adapt<AuthUserDto>()
        };
    }

    public async Task<AuthResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _authRepository.FindByIdAsync(userId);
        if (user is null)
        {
            return Fail("User not found.");
        }

        var result = await _authRepository.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return Fail(GetErrors(result));
        }

        return new AuthResponse
        {
            Success = true,
            Message = "Password changed successfully.",
            User = user.Adapt<AuthUserDto>()
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshToken = await _authRepository.GetRefreshTokenAsync(request.RefreshToken);
        if (refreshToken is null || !refreshToken.IsActive)
        {
            return Fail("Invalid or expired refresh token.");
        }

        var user = await _authRepository.FindByIdAsync(refreshToken.UserId);
        if (user is null)
        {
            return Fail("User not found.");
        }

        // Revoke old refresh token
        await _authRepository.RevokeRefreshTokenAsync(refreshToken);

        // Generate new tokens
        var jwtToken = await BuildJwtAsync(user);
        var newRefreshToken = GenerateRefreshToken(user.Id, jwtToken.Token);
        await _authRepository.CreateRefreshTokenAsync(newRefreshToken);

        return new AuthResponse
        {
            Success = true,
            Message = "Token refreshed successfully.",
            Token = jwtToken.Token,
            ExpiresAt = jwtToken.ExpiresAt,
            User = user.Adapt<AuthUserDto>()
        };
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user, string message)
    {
        var token = await BuildJwtAsync(user);
        var refreshToken = GenerateRefreshToken(user.Id, token.Token);
        await _authRepository.CreateRefreshTokenAsync(refreshToken);

        return new AuthResponse
        {
            Success = true,
            Message = message,
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            User = user.Adapt<AuthUserDto>()
        };
    }
    
    private async Task SendConfirmationEmailAsync(ApplicationUser user, HttpRequest requestHttp)
    {
        try
        {
            _logger.LogInformation($"[EMAIL] Starting SendConfirmationEmailAsync for {user.Email}");
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new InvalidOperationException("User email is missing.");
            }

            _logger.LogInformation($"[EMAIL] Generating confirmation token for {user.Email}");
            var token = await _authRepository.GenerateEmailConfirmationTokenAsync(user);
            _logger.LogInformation($"[EMAIL] Token generated: {token.Substring(0, Math.Min(20, token.Length))}...");
            
            // Convert standard Base64 → Base64Url (URL-safe: no +, /, or = chars)
            var urlSafeToken = token.Replace('+', '-').Replace('/', '_').TrimEnd('=');
            var encodedEmail = Uri.EscapeDataString(user.Email);
            var emailUrl = $"{requestHttp.Scheme}://{requestHttp.Host}/api/Identity/auth/confirm-email?email={encodedEmail}&token={urlSafeToken}";
            _logger.LogInformation($"[EMAIL] Confirmation URL built: {emailUrl}");
            
            var emailBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #c4a747; color: white; padding: 20px; text-align: center; border-radius: 5px; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; margin: 20px 0; border-radius: 5px; }}
                        .button {{ display: inline-block; background-color: #c4a747; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ font-size: 12px; color: #666; text-align: center; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Email Confirmation - BakerGroup</h2>
                        </div>
                        <div class='content'>
                            <p>Hello {user.UserName ?? user.Email},</p>
                            <p>Please confirm your email address to activate your account.</p>
                            <p>
                                <a href='{emailUrl}' class='button'>Confirm Email Address</a>
                            </p>
                            <p><strong>This link will expire in 24 hours.</strong></p>
                        </div>
                        <div class='footer'>
                            <p>If you didn't request this email, please ignore it.</p>
                            <p>&copy; 2026 BakerGroup. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
            
            _logger.LogInformation($"[EMAIL] Email body prepared. Now calling _emailSender.SendEmailAsync for {user.Email}");
            await _emailSender.SendEmailAsync(user.Email, "Confirm your BakerGroup email address", emailBody);
            _logger.LogInformation($"[EMAIL] ✓ Email sent successfully to {user.Email}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[EMAIL-ERROR] Exception in SendConfirmationEmailAsync for {user.Email}: {ex.GetType().FullName} - {ex.Message}. StackTrace: {ex.StackTrace}");
            throw;
        }
    }
    
    private Task<(string Token, DateTime ExpiresAt)> BuildJwtAsync(ApplicationUser user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("JWT key is missing.");
        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("JWT issuer is missing.");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("JWT audience is missing.");
        var expiresMinutes = int.TryParse(jwtSection["ExpiresMinutes"], out var parsedMinutes) ? parsedMinutes : 60;

        var expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.GivenName, user.FullName),
            new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return Task.FromResult((new JwtSecurityTokenHandler().WriteToken(token), expiresAt));
    }

    private RefreshToken GenerateRefreshToken(string userId, string jwtId)
    {
        var refreshTokenExpiration = int.TryParse(_configuration["Jwt:RefreshTokenExpiresMinutes"], out var parsedMinutes) ? parsedMinutes : 1440;

        return new RefreshToken
        {
            Token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64)),
            JwtId = jwtId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(refreshTokenExpiration),
            RevokedAt = null
        };
    }

    private static AuthResponse Fail(string message)
    {
        return new AuthResponse
        {
            Success = false,
            Message = message,
            Errors = [message]
        };
    }

    private static AuthResponse Fail(IEnumerable<string> errors)
    {
        var errorList = errors.ToList();
        return new AuthResponse
        {
            Success = false,
            Message = errorList.FirstOrDefault() ?? "An unexpected error occurred.",
            Errors = errorList
        };
    }

    private static IEnumerable<string> GetErrors(IdentityResult result)
    {
        return result.Errors.Select(error => error.Description);
    }
}


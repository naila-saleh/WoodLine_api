using WoodLine.BLL.Services.Interfaces;
using WoodLine.DAL.DTOs.Requests;
using WoodLine.DAL.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WoodLine.PL.Areas.Identity.Controllers;

[Area("Identity")]
[ApiController]
[Route("api/[area]/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "bgr_refresh_token";
    private const string RefreshTokenCookiePath = "/api/Identity/auth";

    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request, Request);
        return ToActionResult(response);
    }

    [HttpGet("confirm-email")]
    public async Task<ActionResult<AuthResponse>> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
    {
        var request = new ConfirmEmailRequest { Email = email, Token = token };
        var response = await _authService.ConfirmEmailAsync(request);
        return ToActionResult(response);
    }

    [HttpPost("resend-confirmation-email")]
    public async Task<ActionResult<AuthResponse>> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest request)
    {
        var response = await _authService.ResendConfirmationEmailAsync(request, Request);
        return ToActionResult(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        SetRefreshTokenCookie(response);
        return ToActionResult(response);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<AuthResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var response = await _authService.ForgotPasswordAsync(request);
        return ToActionResult(response);
    }

    [HttpPatch("reset-password")]
    public async Task<ActionResult<AuthResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var response = await _authService.ResetPasswordAsync(request);
        return ToActionResult(response);
    }

    [HttpPatch("change-password")]
    [Authorize]
    public async Task<ActionResult<AuthResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var response = await _authService.ChangePasswordAsync(userId, request);
        return ToActionResult(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            ClearRefreshTokenCookie();
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Refresh token cookie is missing.",
                Errors = ["Refresh token cookie is missing."]
            });
        }

        var response = await _authService.RefreshTokenAsync(refreshToken);
        if (response.Success)
        {
            SetRefreshTokenCookie(response);
        }
        else
        {
            ClearRefreshTokenCookie();
        }

        return ToActionResult(response);
    }

    [HttpPost("logout")]
    public async Task<ActionResult<AuthResponse>> Logout()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        var response = await _authService.LogoutAsync(refreshToken ?? string.Empty);

        ClearRefreshTokenCookie();
        return Ok(response);
    }

    private ActionResult<AuthResponse> ToActionResult(AuthResponse response)
    {
        return response.Success ? Ok(response) : BadRequest(response);
    }

    private void SetRefreshTokenCookie(AuthResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.RefreshToken) || response.RefreshTokenExpiresAt is null)
        {
            return;
        }

        Response.Cookies.Append(RefreshTokenCookieName, response.RefreshToken, BuildCookieOptions(response.RefreshTokenExpiresAt.Value));
        response.RefreshToken = null;
        response.RefreshTokenExpiresAt = null;
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            Path = RefreshTokenCookiePath,
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });
    }

    private static CookieOptions BuildCookieOptions(DateTime expiresAt)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = RefreshTokenCookiePath,
            Expires = new DateTimeOffset(DateTime.SpecifyKind(expiresAt, DateTimeKind.Utc))
        };
    }
}



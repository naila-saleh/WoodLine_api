using BakerGroup.BLL.Services.Interfaces;
using BakerGroup.DAL.DTOs.Requests;
using BakerGroup.DAL.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BakerGroup.PL.Areas.Identity.Controllers;

[Area("Identity")]
[ApiController]
[Route("api/[area]/auth")]
public class AuthController : ControllerBase
{
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
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        return ToActionResult(response);
    }

    private ActionResult<AuthResponse> ToActionResult(AuthResponse response)
    {
        return response.Success ? Ok(response) : BadRequest(response);
    }
}



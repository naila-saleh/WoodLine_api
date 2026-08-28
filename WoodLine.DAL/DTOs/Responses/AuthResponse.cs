using System.Text.Json.Serialization;

namespace WoodLine.DAL.DTOs.Responses;

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public AuthUserDto? User { get; set; }
    public IEnumerable<string> Errors { get; set; } = [];
    public string? PasswordResetToken { get; set; }

    [JsonIgnore]
    public string? RefreshToken { get; set; }

    [JsonIgnore]
    public DateTime? RefreshTokenExpiresAt { get; set; }
}


using System.ComponentModel.DataAnnotations;

namespace BakerGroup.DAL.DTOs.Requests;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

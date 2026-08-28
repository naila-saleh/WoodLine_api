using System.ComponentModel.DataAnnotations;

namespace WoodLine.DAL.DTOs.Requests;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

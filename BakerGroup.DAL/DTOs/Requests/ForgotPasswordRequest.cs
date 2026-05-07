using System.ComponentModel.DataAnnotations;

namespace BakerGroup.DAL.DTOs.Requests;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}


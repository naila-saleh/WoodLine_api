using System.ComponentModel.DataAnnotations;

namespace WoodLine.DAL.DTOs.Requests;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}


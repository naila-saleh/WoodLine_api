namespace BakerGroup.DAL.DTOs.Requests;

public class ConfirmEmailRequest
{
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
}


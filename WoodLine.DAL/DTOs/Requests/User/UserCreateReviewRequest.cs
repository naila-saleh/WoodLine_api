using System.ComponentModel.DataAnnotations;

namespace WoodLine.DAL.DTOs.Requests.User;

public class UserCreateReviewRequest
{
    [Required]
    [Range(1, 5)]
    public int Rate { get; set; }
    [Required]
    public string Comment { get; set; } = string.Empty;
    [Required]
    public string ProductId { get; set; } = string.Empty;
}

using WoodLine.DAL.Models;

namespace WoodLine.DAL.DTOs.Responses.Admin;

public class AdminReviewResponse
{
    public string Id { get; set; } = string.Empty;
    public int Rate { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

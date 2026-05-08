namespace BakerGroup.DAL.Models;

public enum Status { Active, Inactive }

public class BaseModel
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Status Status { get; set; }
}
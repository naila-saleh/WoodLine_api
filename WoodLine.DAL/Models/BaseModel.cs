namespace WoodLine.DAL.Models;

public enum Status { Active, Inactive }

public class BaseModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Status Status { get; set; } = Status.Active;
}
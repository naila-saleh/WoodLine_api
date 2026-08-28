namespace WoodLine.DAL.Models;

public class Category : BaseModel
{
    public string Name { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string Image { get; set; }
    public List<Product> Products { get; set; } = new List<Product>();
}
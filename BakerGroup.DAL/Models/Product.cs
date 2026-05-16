namespace BakerGroup.DAL.Models;

public class Product : BaseModel
{
    public string Name { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string Description { get; set; }
    public string DescriptionAr { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public int Quantity { get; set; }
    public string MainImage { get; set; }
    public double Rate { get; set; }
    public string CategoryId { get; set; }
    public Category Category { get; set; }
    public List<ProductImage> SubImages { get; set; } = new List<ProductImage>();
    public List<Review> Reviews { get; set; } = new List<Review>();
}
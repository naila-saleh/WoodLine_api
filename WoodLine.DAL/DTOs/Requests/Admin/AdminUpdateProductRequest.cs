using Microsoft.AspNetCore.Http;
using WoodLine.DAL.Models;

namespace WoodLine.DAL.DTOs.Requests.Admin;

public class AdminUpdateProductRequest
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public decimal? Price { get; set; }
    public decimal? Discount { get; set; }
    public int? Quantity { get; set; }
    public string? CategoryId { get; set; }
    public Status? Status { get; set; }
    public IFormFile? MainImage { get; set; }
    public IFormFileCollection? SubImages { get; set; }
}

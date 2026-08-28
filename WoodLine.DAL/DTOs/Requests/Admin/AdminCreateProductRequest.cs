using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using WoodLine.DAL.Models;

namespace WoodLine.DAL.DTOs.Requests.Admin;

public class AdminCreateProductRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    [Required]
    public decimal Price { get; set; }
    public decimal? Discount { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    public string CategoryId { get; set; } = string.Empty;
    public Status Status { get; set; }
    [Required]
    public IFormFile MainImage { get; set; }
    public IFormFileCollection? SubImages { get; set; }
}

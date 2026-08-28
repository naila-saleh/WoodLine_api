using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using WoodLine.DAL.Models;

namespace WoodLine.DAL.DTOs.Requests.Admin;

public class AdminCreateCategoryRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    [Required]
    public IFormFile Image { get; set; }
    public Status Status { get; set; }
}

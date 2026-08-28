using Microsoft.AspNetCore.Http;
using WoodLine.DAL.Models;

namespace WoodLine.DAL.DTOs.Requests.Admin;

public class AdminUpdateCategoryRequest
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public IFormFile? Image { get; set; }
    public Status? Status { get; set; }
}

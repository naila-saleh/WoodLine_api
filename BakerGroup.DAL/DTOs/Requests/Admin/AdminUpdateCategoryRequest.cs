using Microsoft.AspNetCore.Http;
using BakerGroup.DAL.Models;

namespace BakerGroup.DAL.DTOs.Requests.Admin;

public class AdminUpdateCategoryRequest
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public IFormFile? Image { get; set; }
    public Status? Status { get; set; }
}

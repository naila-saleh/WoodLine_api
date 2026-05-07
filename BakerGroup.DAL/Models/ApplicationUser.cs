using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BakerGroup.DAL.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
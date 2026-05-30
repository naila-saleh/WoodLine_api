using BakerGroup.DAL.Data;
using BakerGroup.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BakerGroup.DAL.Utilities;

public class SeedData : ISeedData
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public SeedData(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }
    public async Task DataSeedingAsync()
    {
        if ((await _context.Database.GetPendingMigrationsAsync()).Any())
        {
            await _context.Database.MigrateAsync();
        }

        // Define the canonical category set with Arabic translations
        var seedCategories = new List<Category>
        {
            new Category{ Name = "Cabinets", NameAr = "الخزائن", Image = "/categories/Cabinet.jpg"},
            new Category{ Name = "Tables", NameAr = "الطاولات", Image = "/categories/Table.jpg"},
            new Category{ Name = "Dining Tables", NameAr = "طاولات الطعام", Image = "/categories/DiningTable.jpg"},
            new Category{ Name = "Sofas", NameAr = "الأرائك", Image = "/categories/Sofa.jpg"},
            new Category{ Name = "Chairs", NameAr = "الكراسي", Image = "/categories/Chair.jpg"},
            new Category{ Name = "Beds", NameAr = "الأسرة", Image = "/categories/Bed.jpg"},
            new Category{ Name = "Desks", NameAr = "المكاتب", Image = "/categories/Desk.jpg"},
            new Category{ Name = "Dressers", NameAr = "التسريحات", Image = "/categories/Dresser.jpg"},
            new Category{ Name = "Outdoor Furniture", NameAr = "الأثاث الخارجي", Image = "/categories/OutdoorFurniture.jpg"},
            new Category{ Name = "Storage", NameAr = "وحدات التخزين", Image = "/categories/Storage.jpg"},
            new Category{ Name = "Kids Furniture", NameAr = "أثاث الأطفال", Image = "/categories/KidsFurniture.jpg"},
            new Category{ Name = "TV Stands", NameAr = "حوامل التلفاز", Image = "/categories/TVStand.jpg"}
        };

        // For idempotency: update existing categories' NameAr (or Image) if missing, or add missing categories
        foreach (var seedCat in seedCategories)
        {
            var existing = await _context.Categories.FirstOrDefaultAsync(c => c.Name == seedCat.Name);
            if (existing == null)
            {
                await _context.Categories.AddAsync(seedCat);
                Console.WriteLine($"[Seed] Added category: {seedCat.Name}");
            }
            else
            {
                var updated = false;
                if (string.IsNullOrWhiteSpace(existing.NameAr) && !string.IsNullOrWhiteSpace(seedCat.NameAr))
                {
                    existing.NameAr = seedCat.NameAr;
                    updated = true;
                }
                if (string.IsNullOrWhiteSpace(existing.Image) && !string.IsNullOrWhiteSpace(seedCat.Image))
                {
                    existing.Image = seedCat.Image;
                    updated = true;
                }
                if (updated)
                {
                    // The entity was queried from the context and is being tracked.
                    // We've modified the properties we want (NameAr/Image), so
                    // there's no need to call Update() which would mark the
                    // entire entity as Modified. Calling SaveChangesAsync() will
                    // persist only the changed properties.
                    Console.WriteLine($"[Seed] Updated category: {existing.Name} (NameAr/Image)");
                }
            }
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task IdentityDataSeedingAsync()
    {
        if (!await _roleManager.Roles.AnyAsync())
        {
            await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
            await _roleManager.CreateAsync(new IdentityRole("User"));
        }

        if (!await _userManager.Users.AnyAsync())
        {
            var user1 = new ApplicationUser()
            {
                Email = "nailasaleh2004@gmail.com",
                FullName = "Naila Saleh",
                PhoneNumber = "+972 56-803-8849",
                UserName = "nailasaleh",
                EmailConfirmed = true
            };
            var user2 = new ApplicationUser()
            {
                Email = "sohyb@bakergroupco.com",
                FullName = "Sohyb Baker",
                PhoneNumber = "+972 59-852-4052",
                UserName = "sohyb",
                EmailConfirmed = true
            };
            var user3 = new ApplicationUser()
            {
                Email = "jawad@gmail.com",
                FullName = "Jawad Saleh",
                PhoneNumber = "+972 59-933-5290",
                UserName = "jawad",
                EmailConfirmed = true
            };

            await _userManager.CreateAsync(user1, "P@ssw0rd");
            await _userManager.CreateAsync(user2, "P@ssw0rd");
            await _userManager.CreateAsync(user3, "P@ssw0rd");

            await _userManager.AddToRoleAsync(user1, "SuperAdmin");
            await _userManager.AddToRoleAsync(user2, "Admin");
            await _userManager.AddToRoleAsync(user3, "User");
        }

        await _context.SaveChangesAsync();
    }
}

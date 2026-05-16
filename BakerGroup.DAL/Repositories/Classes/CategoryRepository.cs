using BakerGroup.DAL.Data;
using BakerGroup.DAL.Models;
using BakerGroup.DAL.Repositories.Interfaces;

namespace BakerGroup.DAL.Repositories.Classes;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }
}

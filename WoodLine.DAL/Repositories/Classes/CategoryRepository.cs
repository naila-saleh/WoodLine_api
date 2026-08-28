using WoodLine.DAL.Data;
using WoodLine.DAL.Models;
using WoodLine.DAL.Repositories.Interfaces;

namespace WoodLine.DAL.Repositories.Classes;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }
}

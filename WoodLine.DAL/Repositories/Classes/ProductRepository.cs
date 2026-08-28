using WoodLine.DAL.Data;
using WoodLine.DAL.Models;
using WoodLine.DAL.Repositories.Interfaces;

namespace WoodLine.DAL.Repositories.Classes;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }
}

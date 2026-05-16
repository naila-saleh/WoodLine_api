using BakerGroup.DAL.Data;
using BakerGroup.DAL.Models;
using BakerGroup.DAL.Repositories.Interfaces;

namespace BakerGroup.DAL.Repositories.Classes;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }
}

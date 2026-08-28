using WoodLine.DAL.Data;
using WoodLine.DAL.Models;
using WoodLine.DAL.Repositories.Interfaces;

namespace WoodLine.DAL.Repositories.Classes;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context)
    {
    }
}

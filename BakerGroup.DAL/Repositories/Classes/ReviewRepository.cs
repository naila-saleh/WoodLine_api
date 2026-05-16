using BakerGroup.DAL.Data;
using BakerGroup.DAL.Models;
using BakerGroup.DAL.Repositories.Interfaces;

namespace BakerGroup.DAL.Repositories.Classes;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    public ReviewRepository(ApplicationDbContext context) : base(context)
    {
    }
}

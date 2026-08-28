using System.Linq.Expressions;
using WoodLine.DAL.Data;
using WoodLine.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace WoodLine.DAL.Repositories.Classes;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
    {
        IQueryable<T> query = _dbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (includeProperties != null)
        {
            foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp);
            }
        }

        return await query.ToListAsync();
    }

    public async Task<(IEnumerable<T> items, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? filter = null, string? includeProperties = null, Expression<Func<T, object>>? orderBy = null)
    {
        IQueryable<T> query = _dbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        // Count before applying includes to avoid counting duplicate rows from joins
        int totalCount = await query.CountAsync();

        if (orderBy != null)
        {
            query = query.OrderByDescending(orderBy);
        }

        // Apply Skip/Take before includes to get correct pagination
        query = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        // Apply includes with AsSplitQuery to avoid duplicate rows from joins
        if (includeProperties != null)
        {
            query = query.AsSplitQuery();
            foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp);
            }
        }

        var items = await query.ToListAsync();

        return (items, totalCount);
    }

    public async Task<T?> GetByIdAsync(string id, string? includeProperties = null)
    {
        IQueryable<T> query = _dbSet;

        if (includeProperties != null)
        {
            foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp);
            }
        }

        // We assume T has an Id property of type string because of BaseModel
        // But since T is generic, we use EF.Property or FindAsync if no includes.
        if (includeProperties == null)
        {
            return await _dbSet.FindAsync(id);
        }

        return await query.FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == id);
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}

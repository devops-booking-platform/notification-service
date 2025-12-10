using NotificationService.Data;
using NotificationService.Repositories.Interfaces;

namespace NotificationService.Repositories;

public class Repository<T>(ApplicationDbContext context) : IRepository<T>
    where T : class
{
    protected readonly ApplicationDbContext Context = context;

    public async Task AddAsync(T entity) =>
        await Context.Set<T>().AddAsync(entity);

    public async Task AddRangeAsync(IEnumerable<T> entities) =>
        await Context.Set<T>().AddRangeAsync(entities);

    public void Remove(T entity) =>
        Context.Set<T>().Remove(entity);

    public Task<T?> GetByIdAsync(Guid id) =>
        Context.Set<T>().FindAsync(id).AsTask();

    public IQueryable<T> Query() =>
        Context.Set<T>();
}
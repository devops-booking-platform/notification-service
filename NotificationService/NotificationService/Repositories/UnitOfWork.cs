using NotificationService.Data;
using NotificationService.Repositories.Interfaces;

namespace NotificationService.Repositories;

public class UnitOfWork(
    ApplicationDbContext context)
    : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
using MandarinAuction.Application.Abstractions.Persistence;
using MandarinAuction.Domain.Entities;
using MandarinAuction.Domain.Enums;
using MandarinAuction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Infrastructure.Persistence;

public class MandarinRepository : IMandarinRepository
{
    private readonly AppDbContext _dbContext;

    public MandarinRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Mandarin mandarin, CancellationToken cancellationToken = default)
    {
        return _dbContext.Mandarins.AddAsync(mandarin, cancellationToken).AsTask();
    }

    public Task<Mandarin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Mandarins.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public Task<Mandarin?> GetByIdWithBidsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Mandarins
            .Include(m => m.Bids)
            .ThenInclude(b => b.User)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public Task<List<Mandarin>> GetActiveWithBidsAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Mandarins
            .AsNoTracking()
            .Where(m => m.Status == MandarinStatus.Active && m.ExpiresAt > DateTime.UtcNow)
            .Include(m => m.Bids)
            .ThenInclude(b => b.User)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Mandarin>> GetExpiredActiveAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Mandarins
            .Where(m => m.Status == MandarinStatus.Active && m.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Mandarin>> GetExpiredActiveWithBidsAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Mandarins
            .Include(m => m.Bids)
            .ThenInclude(b => b.User)
            .Where(m => m.Status == MandarinStatus.Active && m.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
}

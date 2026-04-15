using MandarinAuction.Application.Abstractions.Persistence;
using MandarinAuction.Domain.Entities;
using MandarinAuction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Infrastructure.Persistence;

public class BidRepository : IBidRepository
{
    private readonly AppDbContext _dbContext;

    public BidRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Bid bid, CancellationToken cancellationToken = default)
    {
        return _dbContext.Bids.AddAsync(bid, cancellationToken).AsTask();
    }

    public Task<List<Bid>> GetForMandarinWithUsersAsync(Guid mandarinId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Bids
            .Where(b => b.MandarinId == mandarinId)
            .Include(b => b.User)
            .ToListAsync(cancellationToken);
    }
}

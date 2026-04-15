using MandarinAuction.Application.Abstractions.Persistence;
using MandarinAuction.Domain.Entities;
using MandarinAuction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Infrastructure.Persistence;

public class AuctionSettingsRepository : IAuctionSettingsRepository
{
    private readonly AppDbContext _dbContext;

    public AuctionSettingsRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AuctionSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.AuctionSettings.FirstAsync(cancellationToken);
    }
}

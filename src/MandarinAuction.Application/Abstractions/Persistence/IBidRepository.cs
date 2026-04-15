using DomainBid = MandarinAuction.Domain.Entities.Bid;

namespace MandarinAuction.Application.Abstractions.Persistence;

public interface IBidRepository
{
    Task AddAsync(DomainBid bid, CancellationToken cancellationToken = default);
    Task<List<DomainBid>> GetForMandarinWithUsersAsync(Guid mandarinId, CancellationToken cancellationToken = default);
}

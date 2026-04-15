using DomainMandarin = MandarinAuction.Domain.Entities.Mandarin;

namespace MandarinAuction.Application.Abstractions.Persistence;

public interface IMandarinRepository
{
    Task AddAsync(DomainMandarin mandarin, CancellationToken cancellationToken = default);
    Task<DomainMandarin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DomainMandarin?> GetByIdWithBidsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<DomainMandarin>> GetActiveWithBidsAsync(CancellationToken cancellationToken = default);
    Task<List<DomainMandarin>> GetExpiredActiveAsync(CancellationToken cancellationToken = default);
    Task<List<DomainMandarin>> GetExpiredActiveWithBidsAsync(CancellationToken cancellationToken = default);
}

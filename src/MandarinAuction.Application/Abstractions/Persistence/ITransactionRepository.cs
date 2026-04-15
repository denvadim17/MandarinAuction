using MandarinAuction.Domain.Entities;

namespace MandarinAuction.Application.Abstractions.Persistence;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
}

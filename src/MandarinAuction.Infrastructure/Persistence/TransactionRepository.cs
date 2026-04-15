using MandarinAuction.Application.Abstractions.Persistence;
using MandarinAuction.Domain.Entities;
using MandarinAuction.Infrastructure.Data;

namespace MandarinAuction.Infrastructure.Persistence;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _dbContext;

    public TransactionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        return _dbContext.Transactions.AddAsync(transaction, cancellationToken).AsTask();
    }
}

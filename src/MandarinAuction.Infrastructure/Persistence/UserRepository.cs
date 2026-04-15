using MandarinAuction.Application.Abstractions.Persistence;
using MandarinAuction.Domain.Entities;
using MandarinAuction.Infrastructure.Data;

namespace MandarinAuction.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppUser?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FindAsync([userId], cancellationToken);
    }
}

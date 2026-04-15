using MandarinAuction.Domain.Entities;

namespace MandarinAuction.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<AppUser?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
}

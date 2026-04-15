using MandarinAuction.Application.Abstractions.Persistence;
using MandarinAuction.Application.Abstractions.Services;
using MandarinAuction.Application.Admin;
using MandarinAuction.Domain.Entities;

namespace MandarinAuction.Application.Services;

public class AuctionSettingsService : IAuctionSettingsService
{
    private readonly IAuctionSettingsRepository _settingsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuctionSettingsService(IAuctionSettingsRepository settingsRepository, IUnitOfWork unitOfWork)
    {
        _settingsRepository = settingsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuctionSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetAsync(cancellationToken);
        return Map(settings);
    }

    public async Task<AuctionSettingsDto> UpdateSettingsAsync(
        UpdateSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await _settingsRepository.GetAsync(cancellationToken);

        if (request.CashbackBasePercent.HasValue)
            settings.CashbackBasePercent = request.CashbackBasePercent.Value;
        if (request.CashbackPerMandarinBonus.HasValue)
            settings.CashbackPerMandarinBonus = request.CashbackPerMandarinBonus.Value;
        if (request.CashbackMaxPercent.HasValue)
            settings.CashbackMaxPercent = request.CashbackMaxPercent.Value;
        if (request.MandarinLifetimeMinutes.HasValue)
            settings.MandarinLifetimeMinutes = request.MandarinLifetimeMinutes.Value;
        if (request.GenerationIntervalMinutes.HasValue)
            settings.GenerationIntervalMinutes = request.GenerationIntervalMinutes.Value;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(settings);
    }

    private static AuctionSettingsDto Map(AuctionSettings settings) =>
        new(
            settings.Id,
            settings.CashbackBasePercent,
            settings.CashbackPerMandarinBonus,
            settings.CashbackMaxPercent,
            settings.MandarinLifetimeMinutes,
            settings.GenerationIntervalMinutes);
}

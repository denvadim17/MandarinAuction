using MandarinAuction.Domain.Enums;

namespace MandarinAuction.Domain.Entities;

public class Mandarin
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal BuyNowPrice { get; set; }
    public MandarinStatus Status { get; set; } = MandarinStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public string? WinnerId { get; set; }
    public AppUser? Winner { get; set; }

    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
}

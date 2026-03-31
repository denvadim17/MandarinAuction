using MandarinAuction.Api.Domain.Enums;

namespace MandarinAuction.Api.DTOs.Mandarin;

public class MandarinDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal BuyNowPrice { get; set; }
    public MandarinStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int BidCount { get; set; }
    public string? HighestBidderName { get; set; }

    public MandarinDto() { }

    public MandarinDto(
        Guid id,
        string name,
        string imageUrl,
        decimal startingPrice,
        decimal currentPrice,
        decimal buyNowPrice,
        MandarinStatus status,
        DateTime createdAt,
        DateTime expiresAt,
        int bidCount,
        string? highestBidderName)
    {
        Id = id;
        Name = name;
        ImageUrl = imageUrl;
        StartingPrice = startingPrice;
        CurrentPrice = currentPrice;
        BuyNowPrice = buyNowPrice;
        Status = status;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        BidCount = bidCount;
        HighestBidderName = highestBidderName;
    }
}

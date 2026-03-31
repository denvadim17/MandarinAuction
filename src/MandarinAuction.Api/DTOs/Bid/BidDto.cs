namespace MandarinAuction.Api.DTOs.Bid;

public class BidDto
{
    public Guid Id { get; set; }
    public Guid MandarinId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }

    public BidDto() { }

    public BidDto(Guid id, Guid mandarinId, string userName, decimal amount, DateTime createdAt)
    {
        Id = id;
        MandarinId = mandarinId;
        UserName = userName;
        Amount = amount;
        CreatedAt = createdAt;
    }
}

using System.ComponentModel.DataAnnotations;

namespace MandarinAuction.Application.Bid;

public class PlaceBidRequest
{
    [Required]
    public Guid MandarinId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
}

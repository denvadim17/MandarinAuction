using System.ComponentModel.DataAnnotations;

namespace MandarinAuction.Api.DTOs.Wallet;

public class DepositRequest
{
    [Required]
    [Range(0.01, 100000)]
    public decimal Amount { get; set; }
}

using Microsoft.AspNetCore.Identity;

namespace MandarinAuction.Api.Domain.Entities;

public class AppUser : IdentityUser
{
    public decimal Balance { get; set; }
    public int TotalPurchasedMandarins { get; set; }

    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Mandarin> WonMandarins { get; set; } = new List<Mandarin>();
}

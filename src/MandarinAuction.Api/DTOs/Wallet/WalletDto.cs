namespace MandarinAuction.Api.DTOs.Wallet;

public class WalletDto
{
    public decimal Balance { get; set; }
    public int TotalPurchased { get; set; }
    public decimal CashbackPercent { get; set; }

    public WalletDto() { }

    public WalletDto(decimal balance, int totalPurchased, decimal cashbackPercent)
    {
        Balance = balance;
        TotalPurchased = totalPurchased;
        CashbackPercent = cashbackPercent;
    }
}

using MandarinAuction.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Api.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Mandarin> Mandarins => Set<Mandarin>();
    public DbSet<Bid> Bids => Set<Bid>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<AuctionSettings> AuctionSettings => Set<AuctionSettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>(e =>
        {
            e.Property(u => u.Balance).HasPrecision(18, 2);
        });

        builder.Entity<Mandarin>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.StartingPrice).HasPrecision(18, 2);
            e.Property(m => m.CurrentPrice).HasPrecision(18, 2);
            e.Property(m => m.BuyNowPrice).HasPrecision(18, 2);
            e.HasOne(m => m.Winner)
                .WithMany(u => u.WonMandarins)
                .HasForeignKey(m => m.WinnerId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(m => m.Status);
            e.HasIndex(m => m.ExpiresAt);
        });

        builder.Entity<Bid>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Amount).HasPrecision(18, 2);
            e.HasOne(b => b.Mandarin)
                .WithMany(m => m.Bids)
                .HasForeignKey(b => b.MandarinId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(b => b.User)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(b => b.MandarinId);
        });

        builder.Entity<Transaction>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Amount).HasPrecision(18, 2);
            e.HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(t => t.UserId);
        });

        builder.Entity<AuctionSettings>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.CashbackBasePercent).HasPrecision(5, 2);
            e.Property(s => s.CashbackPerMandarinBonus).HasPrecision(5, 2);
            e.Property(s => s.CashbackMaxPercent).HasPrecision(5, 2);
            e.HasData(new AuctionSettings());
        });
    }
}

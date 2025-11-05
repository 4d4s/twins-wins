using Microsoft.EntityFrameworkCore;
using TwinsWins.Api.Data.Models;

namespace TwinsWins.Api.Data;

/// <summary>
/// Database context for TwinsWins application
/// </summary>
public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<GameLobbyEntity> GameLobbies { get; set; }
    public DbSet<GameTransactionEntity> GameTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.WalletAddress).IsUnique();
            entity.Property(e => e.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // GameLobby configuration
        modelBuilder.Entity<GameLobbyEntity>(entity =>
        {
            entity.HasOne(e => e.Owner)
                  .WithMany(u => u.OwnedGames)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Stake);
            entity.HasIndex(e => e.Created);
            entity.Property(e => e.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // GameTransaction configuration
        modelBuilder.Entity<GameTransactionEntity>(entity =>
        {
            entity.HasOne(e => e.Owner)
                  .WithMany(u => u.OwnedTransactions)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Opponent)
                  .WithMany(u => u.OpponentTransactions)
                  .HasForeignKey(e => e.OpponentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Winner)
                  .WithMany()
                  .HasForeignKey(e => e.WinnerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.OpponentId);
            entity.HasIndex(e => e.Created);
            entity.Property(e => e.Created).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
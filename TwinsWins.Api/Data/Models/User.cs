using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwinsWins.Api.Data.Models;

/// <summary>
/// User entity representing a player in the system
/// </summary>
[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("wallet_address")]
    [MaxLength(68)]
    public string WalletAddress { get; set; } = string.Empty;

    [Required]
    [Column("username")]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Column("created")]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    [Column("last_active")]
    public DateTime? LastActive { get; set; }

    // Navigation properties
    public ICollection<GameLobbyEntity> OwnedGames { get; set; } = new List<GameLobbyEntity>();
    public ICollection<GameTransactionEntity> OwnedTransactions { get; set; } = new List<GameTransactionEntity>();
    public ICollection<GameTransactionEntity> OpponentTransactions { get; set; } = new List<GameTransactionEntity>();
}
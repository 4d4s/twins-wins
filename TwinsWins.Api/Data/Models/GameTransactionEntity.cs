using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwinsWins.Api.Data.Models;

/// <summary>
/// Entity representing a game transaction (paid game instance)
/// </summary>
[Table("game_transactions")]
public class GameTransactionEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("opponent_id")]
    public long? OpponentId { get; set; }

    [Required]
    [Column("stake", TypeName = "decimal(18,6)")]
    public decimal Stake { get; set; }

    [Column("owner_score")]
    public int? OwnerScore { get; set; }

    [Column("opponent_score")]
    public int? OpponentScore { get; set; }

    [Column("winner_id")]
    public long? WinnerId { get; set; }

    [Column("prize_amount", TypeName = "decimal(18,6)")]
    public decimal? PrizeAmount { get; set; }

    [Column("transaction_hash")]
    [MaxLength(100)]
    public string? TransactionHash { get; set; }

    [Required]
    [Column("created")]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    [Column("completed")]
    public DateTime? Completed { get; set; }

    // Navigation properties
    [ForeignKey("OwnerId")]
    public User? Owner { get; set; }

    [ForeignKey("OpponentId")]
    public User? Opponent { get; set; }

    [ForeignKey("WinnerId")]
    public User? Winner { get; set; }
}
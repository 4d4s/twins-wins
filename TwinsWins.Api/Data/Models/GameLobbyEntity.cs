using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwinsWins.Api.Data.Models;

/// <summary>
/// Entity representing a game waiting for an opponent in the lobby
/// </summary>
[Table("game_lobby")]
public class GameLobbyEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Required]
    [Column("stake", TypeName = "decimal(18,6)")]
    public decimal Stake { get; set; }

    [Required]
    [Column("created")]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("body")]
    public string Body { get; set; } = string.Empty; // JSON string containing game data

    // Navigation property
    [ForeignKey("OwnerId")]
    public User? Owner { get; set; }
}
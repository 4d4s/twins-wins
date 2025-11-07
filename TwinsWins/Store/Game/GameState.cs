using TwinsWins.Shared.Models;

namespace TwinsWins.Client.Store.Game;

/// <summary>
/// State for the game feature
/// Contains all data needed to render the game UI
/// </summary>
public record GameState
{
    /// <summary>
    /// The game ID (0 for free games)
    /// </summary>
    public long? CurrentGameId { get; init; }

    /// <summary>
    /// All cells in the game board
    /// </summary>
    public List<Cell> Cells { get; init; } = new();

    /// <summary>
    /// Currently selected cells (max 2)
    /// </summary>
    public List<Cell> SelectedCells { get; init; } = new();

    /// <summary>
    /// IDs of cells that have been matched
    /// </summary>
    public List<int> MatchedCellIds { get; init; } = new();

    /// <summary>
    /// Map of cell ID to image ID (for checking matches)
    /// </summary>
    public Dictionary<int, int> ImageIdMap { get; init; } = new();

    /// <summary>
    /// Number of pairs matched (0-9)
    /// </summary>
    public int MatchedPairs => MatchedCellIds.Count / 2;

    /// <summary>
    /// Player's current score
    /// </summary>
    public int Score { get; init; }

    /// <summary>
    /// Time remaining in seconds (starts at 60)
    /// </summary>
    public int TimeRemaining { get; init; } = 60;

    /// <summary>
    /// Whether the countdown is active (3, 2, 1 before game starts)
    /// </summary>
    public bool IsCountdownActive { get; init; }

    /// <summary>
    /// Current countdown value (3, 2, 1)
    /// </summary>
    public int CountdownValue { get; init; } = 3;

    /// <summary>
    /// Whether the game is currently active (started and not finished)
    /// </summary>
    public bool IsGameActive { get; init; }

    /// <summary>
    /// Whether the game is completed (all pairs matched)
    /// </summary>
    public bool IsGameComplete { get; init; }

    /// <summary>
    /// Whether currently processing a match check
    /// </summary>
    public bool IsProcessingMatch { get; init; }

    /// <summary>
    /// Whether to show images (during preview/countdown)
    /// </summary>
    public bool ShouldShowImages { get; init; }

    /// <summary>
    /// Whether the game is currently loading
    /// </summary>
    public bool IsLoading { get; init; }

    /// <summary>
    /// Error message if something went wrong
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// The player's wallet address (for paid games)
    /// </summary>
    public string? WalletAddress { get; init; }

    /// <summary>
    /// The stake amount (for paid games)
    /// </summary>
    public decimal Stake { get; init; }
}
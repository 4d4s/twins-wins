using TwinsWins.Shared.Models;
using Fluxor;

namespace TwinsWins.Client.Store.Game;

/// <summary>
/// Central game state managed by Fluxor.
/// Using 'record' for immutability - essential for Redux pattern.
/// </summary>
[FeatureState]
public record GameState
{
    // Game Status
    public bool IsLoading { get; init; }
    public bool IsGameActive { get; init; }
    public bool IsCountdownActive { get; init; }
    public int CountdownValue { get; init; }

    // Game Data
    public long? CurrentGameId { get; init; }
    public List<Cell> Cells { get; init; } = new();
    public Dictionary<int, int> ImageIdMap { get; init; } = new();

    // Selected Cells
    public Cell? FirstSelectedCell { get; init; }
    public Cell? SecondSelectedCell { get; init; }

    // Game Progress
    public int Score { get; init; }
    public int TimeRemaining { get; init; }
    public DateTime? GameStartTime { get; init; }

    // Error Handling
    public string? ErrorMessage { get; init; }

    // Computed Properties
    public bool ShouldShowImages => !IsCountdownActive && IsGameActive;
    public bool IsProcessingMatch => FirstSelectedCell != null && SecondSelectedCell != null;
    public int MatchedPairs => Cells.Count(c => c.IsMatched) / 2;
    public bool IsGameComplete => Cells.Any() && Cells.All(c => c.IsMatched);

    // Default constructor for Fluxor
    public GameState() { }
}
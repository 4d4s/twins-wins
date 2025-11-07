using Fluxor;
using TwinsWins.Shared.Models;

namespace TwinsWins.Client.Store.Game;

/// <summary>
/// Feature registration for Game state
/// This tells Fluxor about the Game state slice
/// </summary>
public class GameFeature : Feature<GameState>
{
    public override string GetName() => "Game";

    protected override GameState GetInitialState() => new GameState
    {
        CurrentGameId = null,
        Cells = new List<Cell>(),
        SelectedCells = new List<Cell>(),
        MatchedCellIds = new List<int>(),
        ImageIdMap = new Dictionary<int, int>(),
        Score = 0,
        TimeRemaining = 60,
        IsCountdownActive = false,
        CountdownValue = 3,
        IsGameActive = false,
        IsGameComplete = false,
        IsProcessingMatch = false,
        ShouldShowImages = false,
        IsLoading = false,
        ErrorMessage = null,
        WalletAddress = null,
        Stake = 0
    };
}
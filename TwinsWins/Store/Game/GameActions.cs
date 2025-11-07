using TwinsWins.Shared.Models;

namespace TwinsWins.Client.Store.Game;

// ============================================================================
// GAME ACTIONS - All actions needed for game state management
// ============================================================================

/// <summary>
/// Action to initialize a free practice game
/// </summary>
public record InitFreeGameAction;

/// <summary>
/// Action dispatched when free game initialization succeeds
/// </summary>
public record InitFreeGameSuccessAction(
    long GameId,
    List<Cell> Cells,
    Dictionary<int, int> ImageIdMap
);

/// <summary>
/// Action dispatched when free game initialization fails
/// </summary>
public record InitFreeGameFailureAction(string ErrorMessage);

/// <summary>
/// Action when a cell is clicked
/// </summary>
public record CellClickedAction(int CellId);

/// <summary>
/// Action to select a cell (after validation)
/// </summary>
public record SelectCellAction(int CellId);

/// <summary>
/// Action to check if two selected cells match
/// </summary>
public record CheckMatchAction(GameState State);

/// <summary>
/// Action dispatched when a match is found
/// </summary>
public record MatchFoundAction;

/// <summary>
/// Action dispatched when selected cells don't match
/// </summary>
public record NoMatchAction;

/// <summary>
/// Action to flip cells back (hide them again)
/// </summary>
public record FlipBackAction;

/// <summary>
/// Action to start the countdown before game begins
/// </summary>
public record StartCountdownAction;

/// <summary>
/// Action to update countdown value
/// </summary>
public record UpdateCountdownAction(int Value);

/// <summary>
/// Action when countdown finishes
/// </summary>
public record CountdownCompleteAction;

/// <summary>
/// Action to start the game timer
/// </summary>
public record StartTimerAction;

/// <summary>
/// Action to update the timer (decrement time remaining)
/// </summary>
public record UpdateTimerAction;

/// <summary>
/// Action to stop the timer
/// </summary>
public record StopTimerAction;

/// <summary>
/// Action to clear error messages
/// </summary>
public record ClearErrorAction;

/// <summary>
/// Action to submit game result
/// </summary>
public record SubmitGameResultAction(
    long GameId,
    string WalletAddress,
    int Score,
    int TimeElapsed
);

/// <summary>
/// Action dispatched when game result submission succeeds
/// </summary>
public record SubmitGameResultSuccessAction;

/// <summary>
/// Action dispatched when game result submission fails
/// </summary>
public record SubmitGameResultFailureAction(string ErrorMessage);

/// <summary>
/// Action to reset/restart the game
/// </summary>
public record ResetGameAction;
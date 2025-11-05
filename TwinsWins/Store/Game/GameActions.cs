using TwinsWins.Shared.Models;

namespace TwinsWins.Client.Store.Game;

/// <summary>
/// Actions for game state management
/// </summary>

// ============ Game Initialization ============
public record InitFreeGameAction();
public record InitPaidGameAction(string WalletAddress, decimal Stake);
public record JoinPaidGameAction(long GameId, string WalletAddress);

public record GameInitializedAction(
    long? GameId,
    List<Cell> Cells,
    Dictionary<int, int> ImageIdMap);

public record GameInitializationFailedAction(string Error);

// ============ Countdown ============
public record StartCountdownAction();
public record CountdownTickAction(int CountdownValue);
public record CountdownCompleteAction();

// ============ Game Flow ============
public record StartGameAction();
public record GameTickAction(int TimeRemaining);
public record EndGameAction(int FinalScore);

// ============ Cell Interaction ============
public record CellClickedAction(int CellId);
public record RevealCellAction(int CellId);
public record CheckMatchAction();
public record MatchFoundAction(int Cell1Id, int Cell2Id, int PointsAwarded);
public record MatchFailedAction(int Cell1Id, int Cell2Id, int PointsDeducted);
public record ResetSelectedCellsAction();

// ============ Score Management ============
public record UpdateScoreAction(int NewScore);
public record SubmitScoreAction(long GameId, string WalletAddress, int Score);
public record ScoreSubmittedAction();
public record ScoreSubmissionFailedAction(string Error);

// ============ Loading & Errors ============
public record SetLoadingAction(bool IsLoading);
public record SetErrorAction(string Error);
public record ClearErrorAction();

// ============ Reset ============
public record ResetGameAction();
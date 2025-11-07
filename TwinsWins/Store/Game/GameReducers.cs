using Fluxor;
using TwinsWins.Shared.Models;

namespace TwinsWins.Client.Store.Game;

/// <summary>
/// Reducers for game state
/// Pure functions that update state based on actions
/// </summary>
public static class GameReducers
{
    // ====================================================================
    // INIT FREE GAME REDUCERS
    // ====================================================================

    [ReducerMethod]
    public static GameState ReduceInitFreeGame(GameState state, InitFreeGameAction action)
    {
        return state with
        {
            IsLoading = true,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static GameState ReduceInitFreeGameSuccess(GameState state, InitFreeGameSuccessAction action)
    {
        return state with
        {
            CurrentGameId = action.GameId,
            Cells = action.Cells,
            ImageIdMap = action.ImageIdMap,
            IsLoading = false,
            IsCountdownActive = true,
            CountdownValue = 3,
            ShouldShowImages = true,
            TimeRemaining = 60,
            Score = 0,
            MatchedCellIds = new List<int>(),
            SelectedCells = new List<Cell>(),
            IsGameComplete = false,
            IsGameActive = false,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static GameState ReduceInitFreeGameFailure(GameState state, InitFreeGameFailureAction action)
    {
        return state with
        {
            IsLoading = false,
            ErrorMessage = action.ErrorMessage
        };
    }

    // ====================================================================
    // COUNTDOWN REDUCERS
    // ====================================================================

    [ReducerMethod]
    public static GameState ReduceStartCountdown(GameState state, StartCountdownAction action)
    {
        return state with
        {
            IsCountdownActive = true,
            CountdownValue = 3,
            ShouldShowImages = true
        };
    }

    [ReducerMethod]
    public static GameState ReduceUpdateCountdown(GameState state, UpdateCountdownAction action)
    {
        return state with
        {
            CountdownValue = action.Value
        };
    }

    [ReducerMethod]
    public static GameState ReduceCountdownComplete(GameState state, CountdownCompleteAction action)
    {
        return state with
        {
            IsCountdownActive = false,
            IsGameActive = true,
            ShouldShowImages = false
        };
    }

    // ====================================================================
    // CELL SELECTION REDUCERS
    // ====================================================================

    [ReducerMethod]
    public static GameState ReduceCellClicked(GameState state, CellClickedAction action)
    {
        // Don't allow selection if game not active
        if (!state.IsGameActive || state.IsGameComplete || state.IsProcessingMatch)
        {
            return state;
        }

        // Find the cell
        var cell = state.Cells.FirstOrDefault(c => c.Id == action.CellId);
        if (cell == null || !cell.IsClickable)
        {
            return state;
        }

        // Don't select if already matched
        if (state.MatchedCellIds.Contains(action.CellId))
        {
            return state;
        }

        // Don't select if already selected
        if (state.SelectedCells.Any(c => c.Id == action.CellId))
        {
            return state;
        }

        // Don't select if already have 2 selected
        if (state.SelectedCells.Count >= 2)
        {
            return state;
        }

        // Update cells to show this one as revealed
        var updatedCells = state.Cells.Select(c =>
            c.Id == action.CellId ? c with { IsRevealed = true } : c
        ).ToList();

        // Add to selected cells
        var newSelectedCells = new List<Cell>(state.SelectedCells) { cell with { IsRevealed = true } };

        // Check if we should process a match
        var shouldProcessMatch = newSelectedCells.Count == 2;

        return state with
        {
            Cells = updatedCells,
            SelectedCells = newSelectedCells,
            IsProcessingMatch = shouldProcessMatch
        };
    }

    [ReducerMethod]
    public static GameState ReduceSelectCell(GameState state, SelectCellAction action)
    {
        // This is now handled by CellClickedAction
        return state;
    }

    // ====================================================================
    // MATCH CHECK REDUCERS
    // ====================================================================

    [ReducerMethod]
    public static GameState ReduceMatchFound(GameState state, MatchFoundAction action)
    {
        // Add selected cells to matched list
        var newMatchedIds = state.MatchedCellIds
            .Concat(state.SelectedCells.Select(c => c.Id))
            .ToList();

        // Update cells to show as matched
        var updatedCells = state.Cells.Select(c =>
            newMatchedIds.Contains(c.Id) ? c with { IsMatched = true, IsRevealed = true } : c
        ).ToList();

        // Calculate new score (100 points per match + time bonus)
        var matchBonus = 100;
        var timeBonus = state.TimeRemaining > 50 ? 50 : state.TimeRemaining;
        var newScore = state.Score + matchBonus + timeBonus;

        // Check if game is completed (all 9 pairs matched)
        var isCompleted = newMatchedIds.Count >= 18;

        return state with
        {
            Cells = updatedCells,
            MatchedCellIds = newMatchedIds,
            SelectedCells = new List<Cell>(),
            Score = newScore,
            IsGameComplete = isCompleted,
            IsGameActive = !isCompleted,
            IsProcessingMatch = false
        };
    }

    [ReducerMethod]
    public static GameState ReduceNoMatch(GameState state, NoMatchAction action)
    {
        // Hide the revealed cards after a delay
        var selectedIds = state.SelectedCells.Select(c => c.Id).ToList();
        var updatedCells = state.Cells.Select(c =>
            selectedIds.Contains(c.Id) ? c with { IsRevealed = false } : c
        ).ToList();

        return state with
        {
            Cells = updatedCells,
            SelectedCells = new List<Cell>(),
            IsProcessingMatch = false
        };
    }

    [ReducerMethod]
    public static GameState ReduceFlipBack(GameState state, FlipBackAction action)
    {
        // Clear selected cells (used for animations)
        return state with
        {
            SelectedCells = new List<Cell>(),
            IsProcessingMatch = false
        };
    }

    // ====================================================================
    // TIMER REDUCERS
    // ====================================================================

    [ReducerMethod]
    public static GameState ReduceStartTimer(GameState state, StartTimerAction action)
    {
        return state with
        {
            TimeRemaining = 60
        };
    }

    [ReducerMethod]
    public static GameState ReduceUpdateTimer(GameState state, UpdateTimerAction action)
    {
        var newTimeRemaining = state.TimeRemaining - 1;
        var gameStillActive = newTimeRemaining > 0 && !state.IsGameComplete;

        return state with
        {
            TimeRemaining = Math.Max(0, newTimeRemaining),
            IsGameActive = gameStillActive
        };
    }

    [ReducerMethod]
    public static GameState ReduceStopTimer(GameState state, StopTimerAction action)
    {
        return state;
    }

    // ====================================================================
    // ERROR HANDLING REDUCERS
    // ====================================================================

    [ReducerMethod]
    public static GameState ReduceClearError(GameState state, ClearErrorAction action)
    {
        return state with
        {
            ErrorMessage = null
        };
    }

    // ====================================================================
    // GAME RESULT REDUCERS
    // ====================================================================

    [ReducerMethod]
    public static GameState ReduceSubmitGameResult(GameState state, SubmitGameResultAction action)
    {
        return state with
        {
            IsLoading = true
        };
    }

    [ReducerMethod]
    public static GameState ReduceSubmitGameResultSuccess(GameState state, SubmitGameResultSuccessAction action)
    {
        return state with
        {
            IsLoading = false
        };
    }

    [ReducerMethod]
    public static GameState ReduceSubmitGameResultFailure(GameState state, SubmitGameResultFailureAction action)
    {
        return state with
        {
            IsLoading = false,
            ErrorMessage = action.ErrorMessage
        };
    }

    // ====================================================================
    // RESET REDUCER
    // ====================================================================

    [ReducerMethod]
    public static GameState ReduceResetGame(GameState state, ResetGameAction action)
    {
        // Reset to initial state
        return new GameState
        {
            CurrentGameId = null,
            Cells = new List<Cell>(),
            SelectedCells = new List<Cell>(),
            MatchedCellIds = new List<int>(),
            ImageIdMap = new Dictionary<int, int>(),
            TimeRemaining = 60,
            Score = 0,
            IsCountdownActive = false,
            CountdownValue = 3,
            IsGameActive = false,
            IsGameComplete = false,
            IsProcessingMatch = false,
            ShouldShowImages = false,
            IsLoading = false,
            ErrorMessage = null
        };
    }
}
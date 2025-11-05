using Fluxor;
using TwinsWins.Shared.Models;

namespace TwinsWins.Client.Store.Game;

/// <summary>
/// Reducers define how state changes in response to actions
/// Pure functions: (State, Action) => NewState
/// </summary>
public static class GameReducers
{
    // ============ Initialization ============

    [ReducerMethod]
    public static GameState ReduceGameInitialized(GameState state, GameInitializedAction action) =>
        state with
        {
            CurrentGameId = action.GameId,
            Cells = action.Cells,
            ImageIdMap = action.ImageIdMap,
            Score = 0,
            TimeRemaining = 60,
            IsLoading = false,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static GameState ReduceGameInitializationFailed(GameState state, GameInitializationFailedAction action) =>
        state with
        {
            IsLoading = false,
            ErrorMessage = action.Error
        };

    // ============ Countdown ============

    [ReducerMethod]
    public static GameState ReduceStartCountdown(GameState state, StartCountdownAction action) =>
        state with
        {
            IsCountdownActive = true,
            CountdownValue = 3,
            IsGameActive = false
        };

    [ReducerMethod]
    public static GameState ReduceCountdownTick(GameState state, CountdownTickAction action) =>
        state with
        {
            CountdownValue = action.CountdownValue
        };

    [ReducerMethod]
    public static GameState ReduceCountdownComplete(GameState state, CountdownCompleteAction action) =>
        state with
        {
            IsCountdownActive = false,
            CountdownValue = 0
        };

    // ============ Game Flow ============

    [ReducerMethod]
    public static GameState ReduceStartGame(GameState state, StartGameAction action) =>
        state with
        {
            IsGameActive = true,
            TimeRemaining = 60,
            GameStartTime = DateTime.UtcNow,
            Score = 0
        };

    [ReducerMethod]
    public static GameState ReduceGameTick(GameState state, GameTickAction action) =>
        state with
        {
            TimeRemaining = action.TimeRemaining
        };

    [ReducerMethod]
    public static GameState ReduceEndGame(GameState state, EndGameAction action) =>
        state with
        {
            IsGameActive = false,
            TimeRemaining = 0
        };

    // ============ Cell Interaction ============

    [ReducerMethod]
    public static GameState ReduceRevealCell(GameState state, RevealCellAction action)
    {
        var cells = state.Cells.Select(c =>
            c.Id == action.CellId ? c with { IsRevealed = true } : c
        ).ToList();

        var revealedCell = cells.First(c => c.Id == action.CellId);

        Cell? firstSelected = state.FirstSelectedCell;
        Cell? secondSelected = state.SecondSelectedCell;

        if (firstSelected == null)
        {
            firstSelected = revealedCell;
        }
        else if (secondSelected == null)
        {
            secondSelected = revealedCell;
        }

        return state with
        {
            Cells = cells,
            FirstSelectedCell = firstSelected,
            SecondSelectedCell = secondSelected
        };
    }

    [ReducerMethod]
    public static GameState ReduceMatchFound(GameState state, MatchFoundAction action)
    {
        var cells = state.Cells.Select(c =>
        {
            if (c.Id == action.Cell1Id || c.Id == action.Cell2Id)
            {
                return c with { IsMatched = true, IsRevealed = false };
            }
            return c;
        }).ToList();

        return state with
        {
            Cells = cells,
            Score = state.Score + action.PointsAwarded,
            FirstSelectedCell = null,
            SecondSelectedCell = null
        };
    }

    [ReducerMethod]
    public static GameState ReduceMatchFailed(GameState state, MatchFailedAction action)
    {
        var cells = state.Cells.Select(c =>
        {
            if (c.Id == action.Cell1Id || c.Id == action.Cell2Id)
            {
                return c with { IsRevealed = false };
            }
            return c;
        }).ToList();

        return state with
        {
            Cells = cells,
            Score = Math.Max(0, state.Score + action.PointsDeducted), // Don't go below 0
            FirstSelectedCell = null,
            SecondSelectedCell = null
        };
    }

    [ReducerMethod]
    public static GameState ReduceResetSelectedCells(GameState state, ResetSelectedCellsAction action) =>
        state with
        {
            FirstSelectedCell = null,
            SecondSelectedCell = null
        };

    // ============ Score Management ============

    [ReducerMethod]
    public static GameState ReduceUpdateScore(GameState state, UpdateScoreAction action) =>
        state with
        {
            Score = action.NewScore
        };

    // ============ Loading & Errors ============

    [ReducerMethod]
    public static GameState ReduceSetLoading(GameState state, SetLoadingAction action) =>
        state with
        {
            IsLoading = action.IsLoading
        };

    [ReducerMethod]
    public static GameState ReduceSetError(GameState state, SetErrorAction action) =>
        state with
        {
            ErrorMessage = action.Error,
            IsLoading = false
        };

    [ReducerMethod]
    public static GameState ReduceClearError(GameState state, ClearErrorAction action) =>
        state with
        {
            ErrorMessage = null
        };

    // ============ Reset ============

    [ReducerMethod]
    public static GameState ReduceResetGame(GameState state, ResetGameAction action) =>
        new GameState();
}
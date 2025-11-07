using Fluxor;
using TwinsWins.Shared.Models;

namespace TwinsWins.Client.Store.Lobby;

/// <summary>
/// Reducers for lobby state
/// Pure functions that update state based on actions
/// </summary>
public static class LobbyReducers
{
    // ====================================================================
    // LOAD GAMES REDUCERS
    // ====================================================================

    [ReducerMethod]
    public static LobbyState ReduceLoadLobbyGames(LobbyState state, LoadLobbyGamesAction action)
    {
        return state with
        {
            IsLoading = true,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static LobbyState ReduceLoadLobbyGamesSuccess(LobbyState state, LoadLobbyGamesSuccessAction action)
    {
        return state with
        {
            AvailableGames = action.Games,
            IsLoading = false,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static LobbyState ReduceLoadLobbyGamesFailure(LobbyState state, LoadLobbyGamesFailureAction action)
    {
        return state with
        {
            IsLoading = false,
            ErrorMessage = action.ErrorMessage
        };
    }

    // Alternative action names for compatibility
    [ReducerMethod]
    public static LobbyState ReduceLoadGames(LobbyState state, LoadGamesAction action)
    {
        return state with
        {
            IsLoading = true,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static LobbyState ReduceLoadGamesSuccess(LobbyState state, LoadGamesSuccessAction action)
    {
        return state with
        {
            AvailableGames = action.Games,
            IsLoading = false,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static LobbyState ReduceLoadGamesFailure(LobbyState state, LoadGamesFailureAction action)
    {
        return state with
        {
            IsLoading = false,
            ErrorMessage = action.ErrorMessage
        };
    }

    // ====================================================================
    // LOAD STATS REDUCERS
    // ====================================================================

    [ReducerMethod]
    public static LobbyState ReduceLoadStats(LobbyState state, LoadStatsAction action)
    {
        return state with
        {
            IsLoading = true
        };
    }

    [ReducerMethod]
    public static LobbyState ReduceLoadStatsSuccess(LobbyState state, LoadStatsSuccessAction action)
    {
        return state with
        {
            Stats = action.Stats,
            IsLoading = false
        };
    }

    [ReducerMethod]
    public static LobbyState ReduceLoadStatsFailure(LobbyState state, LoadStatsFailureAction action)
    {
        return state with
        {
            IsLoading = false,
            ErrorMessage = action.ErrorMessage
        };
    }

    // ====================================================================
    // PAGINATION REDUCERS
    // ====================================================================

    [ReducerMethod]
    public static LobbyState ReduceGoToPreviousPage(LobbyState state, GoToPreviousPageAction action)
    {
        if (state.CurrentPage <= 1)
            return state;

        return state with
        {
            CurrentPage = state.CurrentPage - 1
        };
    }

    [ReducerMethod]
    public static LobbyState ReduceGoToNextPage(LobbyState state, GoToNextPageAction action)
    {
        if (state.CurrentPage >= state.TotalPages)
            return state;

        return state with
        {
            CurrentPage = state.CurrentPage + 1
        };
    }

    [ReducerMethod]
    public static LobbyState ReduceSetPage(LobbyState state, SetPageAction action)
    {
        if (action.PageNumber < 1 || action.PageNumber > state.TotalPages)
            return state;

        return state with
        {
            CurrentPage = action.PageNumber
        };
    }

    // ====================================================================
    // AUTO-REFRESH REDUCERS
    // ====================================================================

    [ReducerMethod]
    public static LobbyState ReduceStartAutoRefresh(LobbyState state, StartAutoRefreshAction action)
    {
        return state with
        {
            IsAutoRefreshing = true
        };
    }

    [ReducerMethod]
    public static LobbyState ReduceStopAutoRefresh(LobbyState state, StopAutoRefreshAction action)
    {
        return state with
        {
            IsAutoRefreshing = false
        };
    }

    // ====================================================================
    // FILTER & SORT REDUCERS
    // ====================================================================

    [ReducerMethod]
    public static LobbyState ReduceApplyFilters(LobbyState state, ApplyFiltersAction action)
    {
        return state with
        {
            MinStakeFilter = action.MinStake,
            MaxStakeFilter = action.MaxStake,
            CurrentPage = 1 // Reset to first page when filters change
        };
    }

    [ReducerMethod]
    public static LobbyState ReduceSetSort(LobbyState state, SetSortAction action)
    {
        return state with
        {
            SortBy = action.SortBy,
            SortAscending = action.Ascending
        };
    }

    // ====================================================================
    // CREATE/JOIN GAME REDUCERS
    // ====================================================================

    [ReducerMethod]
    public static LobbyState ReduceCreateGame(LobbyState state, CreateGameAction action)
    {
        return state with
        {
            IsLoading = true,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static LobbyState ReduceCreateGameSuccess(LobbyState state, CreateGameSuccessAction action)
    {
        return state with
        {
            IsLoading = false
        };
    }

    [ReducerMethod]
    public static LobbyState ReduceCreateGameFailure(LobbyState state, CreateGameFailureAction action)
    {
        return state with
        {
            IsLoading = false,
            ErrorMessage = action.ErrorMessage
        };
    }

    [ReducerMethod]
    public static LobbyState ReduceJoinGame(LobbyState state, JoinGameAction action)
    {
        return state with
        {
            IsLoading = true,
            ErrorMessage = null
        };
    }

    [ReducerMethod]
    public static LobbyState ReduceJoinGameSuccess(LobbyState state, JoinGameSuccessAction action)
    {
        return state with
        {
            IsLoading = false
        };
    }

    [ReducerMethod]
    public static LobbyState ReduceJoinGameFailure(LobbyState state, JoinGameFailureAction action)
    {
        return state with
        {
            IsLoading = false,
            ErrorMessage = action.ErrorMessage
        };
    }
}
using Fluxor;
using TwinsWins.Shared.Models;

namespace TwinsWins.Client.Store.Lobby;

// ============ STATE ============

[FeatureState]
public record LobbyState
{
    public List<GameLobby> AvailableGames { get; init; } = new();
    public bool IsLoading { get; init; }
    public string? ErrorMessage { get; init; }
    public int CurrentPage { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public int TotalPages => (int)Math.Ceiling((double)AvailableGames.Count / PageSize);
    public List<GameLobby> CurrentPageGames =>
        AvailableGames
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

    public LobbyState() { }
}

// ============ ACTIONS ============

// Fetch available games
public record LoadLobbyGamesAction();
public record LobbyGamesLoadedAction(List<GameLobby> Games);
public record LobbyGamesLoadFailedAction(string Error);

// Real-time updates from SignalR
public record GameAddedToLobbyAction(GameLobby Game);
public record GameRemovedFromLobbyAction(long GameId);

// Pagination
public record GoToNextPageAction();
public record GoToPreviousPageAction();
public record SetPageAction(int Page);

// Loading & Errors
public record SetLobbyLoadingAction(bool IsLoading);
public record ClearLobbyErrorAction();

// ============ REDUCERS ============

public static class LobbyReducers
{
    [ReducerMethod]
    public static LobbyState ReduceLobbyGamesLoaded(LobbyState state, LobbyGamesLoadedAction action) =>
        state with
        {
            AvailableGames = action.Games,
            IsLoading = false,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static LobbyState ReduceLobbyGamesLoadFailed(LobbyState state, LobbyGamesLoadFailedAction action) =>
        state with
        {
            IsLoading = false,
            ErrorMessage = action.Error
        };

    [ReducerMethod]
    public static LobbyState ReduceGameAddedToLobby(LobbyState state, GameAddedToLobbyAction action)
    {
        var games = state.AvailableGames.ToList();
        games.Insert(0, action.Game); // Add to beginning
        return state with { AvailableGames = games };
    }

    [ReducerMethod]
    public static LobbyState ReduceGameRemovedFromLobby(LobbyState state, GameRemovedFromLobbyAction action)
    {
        var games = state.AvailableGames.Where(g => g.Id != action.GameId).ToList();
        return state with { AvailableGames = games };
    }

    [ReducerMethod]
    public static LobbyState ReduceGoToNextPage(LobbyState state, GoToNextPageAction action)
    {
        if (state.CurrentPage < state.TotalPages)
        {
            return state with { CurrentPage = state.CurrentPage + 1 };
        }
        return state;
    }

    [ReducerMethod]
    public static LobbyState ReduceGoToPreviousPage(LobbyState state, GoToPreviousPageAction action)
    {
        if (state.CurrentPage > 1)
        {
            return state with { CurrentPage = state.CurrentPage - 1 };
        }
        return state;
    }

    [ReducerMethod]
    public static LobbyState ReduceSetPage(LobbyState state, SetPageAction action) =>
        state with
        {
            CurrentPage = Math.Max(1, Math.Min(action.Page, state.TotalPages))
        };

    [ReducerMethod]
    public static LobbyState ReduceSetLobbyLoading(LobbyState state, SetLobbyLoadingAction action) =>
        state with
        {
            IsLoading = action.IsLoading
        };

    [ReducerMethod]
    public static LobbyState ReduceClearLobbyError(LobbyState state, ClearLobbyErrorAction action) =>
        state with
        {
            ErrorMessage = null
        };
}
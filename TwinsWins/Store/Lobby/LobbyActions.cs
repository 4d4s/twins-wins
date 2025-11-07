using TwinsWins.Shared.Models;
using TwinsWins.Shared.DTOs;

namespace TwinsWins.Client.Store.Lobby;

// ============================================================================
// LOBBY ACTIONS - All actions needed for lobby state management
// ============================================================================

/// <summary>
/// Action to load available games from lobby
/// </summary>
public record LoadLobbyGamesAction;

/// <summary>
/// Action dispatched when loading games succeeds
/// </summary>
public record LoadLobbyGamesSuccessAction(List<GameLobby> Games);

/// <summary>
/// Action dispatched when loading games fails
/// </summary>
public record LoadLobbyGamesFailureAction(string ErrorMessage);

/// <summary>
/// Action to load games (alternative name for compatibility)
/// </summary>
public record LoadGamesAction;

/// <summary>
/// Action dispatched when games are loaded successfully
/// </summary>
public record LoadGamesSuccessAction(List<GameLobby> Games);

/// <summary>
/// Action dispatched when loading games fails
/// </summary>
public record LoadGamesFailureAction(string ErrorMessage);

/// <summary>
/// Action to load lobby statistics
/// </summary>
public record LoadStatsAction;

/// <summary>
/// Action dispatched when stats are loaded successfully
/// </summary>
public record LoadStatsSuccessAction(LobbyStatsResponse Stats);

/// <summary>
/// Action dispatched when loading stats fails
/// </summary>
public record LoadStatsFailureAction(string ErrorMessage);

/// <summary>
/// Action to create a new paid game
/// </summary>
public record CreateGameAction(
    string WalletAddress,
    decimal Stake
);

/// <summary>
/// Action dispatched when game is created successfully
/// </summary>
public record CreateGameSuccessAction(
    long GameId,
    List<Cell> Cells,
    Dictionary<int, int> ImageIdMap
);

/// <summary>
/// Action dispatched when game creation fails
/// </summary>
public record CreateGameFailureAction(string ErrorMessage);

/// <summary>
/// Action to join an existing game
/// </summary>
public record JoinGameAction(
    long GameId,
    string WalletAddress
);

/// <summary>
/// Action dispatched when joining game succeeds
/// </summary>
public record JoinGameSuccessAction(
    long GameId,
    List<Cell> Cells,
    Dictionary<int, int> ImageIdMap
);

/// <summary>
/// Action dispatched when joining game fails
/// </summary>
public record JoinGameFailureAction(string ErrorMessage);

/// <summary>
/// Action to initialize a paid game (from lobby)
/// </summary>
public record InitPaidGameAction(string WalletAddress, decimal Stake);

/// <summary>
/// Action to join a paid game (from lobby)
/// </summary>
public record JoinPaidGameAction(long GameId, string WalletAddress);

// ====================================================================
// PAGINATION ACTIONS
// ====================================================================

/// <summary>
/// Action to go to the previous page
/// </summary>
public record GoToPreviousPageAction;

/// <summary>
/// Action to go to the next page
/// </summary>
public record GoToNextPageAction;

/// <summary>
/// Action to set a specific page
/// </summary>
public record SetPageAction(int PageNumber);

// ====================================================================
// FILTER & SORT ACTIONS
// ====================================================================

/// <summary>
/// Action to apply stake filters
/// </summary>
public record ApplyFiltersAction(decimal? MinStake, decimal? MaxStake);

/// <summary>
/// Action to set sort order
/// </summary>
public record SetSortAction(string SortBy, bool Ascending);

/// <summary>
/// Action to filter games by stake (alternative)
/// </summary>
public record FilterGamesByStakeAction(decimal? MinStake, decimal? MaxStake);

/// <summary>
/// Action to sort games (alternative)
/// </summary>
public record SortGamesAction(string SortBy, bool Ascending);

// ====================================================================
// AUTO-REFRESH ACTIONS
// ====================================================================

/// <summary>
/// Action to start auto-refresh of lobby data
/// </summary>
public record StartAutoRefreshAction;

/// <summary>
/// Action to stop auto-refresh of lobby data
/// </summary>
public record StopAutoRefreshAction;
using TwinsWins.Shared.DTOs;
using TwinsWins.Shared.Models;

namespace TwinsWins.Client.Store.Lobby;

/// <summary>
/// State for the lobby feature
/// Contains all data needed to render the lobby UI
/// </summary>
public record LobbyState
{
    /// <summary>
    /// List of available games in the lobby
    /// </summary>
    public List<GameLobby> AvailableGames { get; init; } = new();

    /// <summary>
    /// Lobby statistics
    /// </summary>
    public LobbyStatsResponse? Stats { get; init; }

    /// <summary>
    /// Whether the lobby is currently loading
    /// </summary>
    public bool IsLoading { get; init; }

    /// <summary>
    /// Error message if something went wrong
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Current page number (for pagination)
    /// </summary>
    public int CurrentPage { get; init; } = 1;

    /// <summary>
    /// Items per page
    /// </summary>
    public int PageSize { get; init; } = 12;

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => AvailableGames.Count > 0 ? (int)Math.Ceiling(AvailableGames.Count / (double)PageSize) : 1;

    /// <summary>
    /// Games for current page
    /// </summary>
    public List<GameLobby> CurrentPageGames => AvailableGames
        .Skip((CurrentPage - 1) * PageSize)
        .Take(PageSize)
        .ToList();

    /// <summary>
    /// Minimum stake filter
    /// </summary>
    public decimal? MinStakeFilter { get; init; }

    /// <summary>
    /// Maximum stake filter
    /// </summary>
    public decimal? MaxStakeFilter { get; init; }

    /// <summary>
    /// Sort field (e.g., "Stake", "CreatedAt")
    /// </summary>
    public string SortBy { get; init; } = "Stake";

    /// <summary>
    /// Sort order (true = ascending, false = descending)
    /// </summary>
    public bool SortAscending { get; init; }

    /// <summary>
    /// Whether auto-refresh is currently running
    /// </summary>
    public bool IsAutoRefreshing { get; init; }
}
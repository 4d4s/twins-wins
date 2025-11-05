using TwinsWins.Shared.DTOs;
using TwinsWins.Shared.Models;

namespace TwinsWins.Api.Services;

/// <summary>
/// Service interface for game operations
/// </summary>
public interface IGameService
{
    // Game Creation
    Task<CreateGameResponse> CreateFreeGame();
    Task<CreateGameResponse> CreatePaidGame(string walletAddress, decimal stake);
    Task<CreateGameResponse> JoinPaidGame(long gameId, string walletAddress);

    // Score Management
    Task<SubmitScoreResponse> SubmitScore(long gameId, string walletAddress, int score, int timeElapsed);

    // Game Information
    Task<GameStatusResponse> GetGameStatus(long gameId);
    Task<GameLobby> GetLobbyGame(long gameId);

    // Game Management
    Task CancelGame(long gameId, string walletAddress);

    // Lobby Operations
    Task<List<GameLobby>> GetAvailableGames(int page = 1, int pageSize = 20);
    Task<List<GameLobby>> GetGamesByOwner(string walletAddress);
    Task<List<GameLobby>> GetGamesByStakeRange(decimal minStake, decimal? maxStake);
    Task<LobbyStatsResponse> GetLobbyStats();
}

/// <summary>
/// Response for score submission
/// </summary>
public class SubmitScoreResponse
{
    public bool IsGameComplete { get; set; }
    public string? Winner { get; set; }
    public int? WinnerScore { get; set; }
    public int? LoserScore { get; set; }
    public decimal? PrizeAmount { get; set; }
    public string? TransactionHash { get; set; }
}

/// <summary>
/// Response for game status
/// </summary>
public class GameStatusResponse
{
    public long GameId { get; set; }
    public string OwnerAddress { get; set; } = string.Empty;
    public string? OpponentAddress { get; set; }
    public int? OwnerScore { get; set; }
    public int? OpponentScore { get; set; }
    public decimal Stake { get; set; }
    public DateTime Created { get; set; }
    public GameStatus Status { get; set; }
    public string? Winner { get; set; }
}

/// <summary>
/// Game status enumeration
/// </summary>
public enum GameStatus
{
    WaitingForOpponent,
    OwnerPlaying,
    OpponentPlaying,
    BothCompleted,
    Cancelled
}
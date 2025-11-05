using TwinsWins.Api.Data.Models;

namespace TwinsWins.Api.Data.Repositories;

/// <summary>
/// Repository interface for GameLobby operations
/// </summary>
public interface IGameLobbyRepository
{
    Task<GameLobbyEntity?> GetByIdAsync(long id);
    Task<GameLobbyEntity> CreateAsync(GameLobbyEntity lobby);
    Task<GameLobbyEntity> UpdateAsync(GameLobbyEntity lobby);
    Task DeleteAsync(long id);
    Task<List<GameLobbyEntity>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<List<GameLobbyEntity>> GetByOwnerIdAsync(long ownerId);
    Task<List<GameLobbyEntity>> GetByStakeRangeAsync(decimal minStake, decimal? maxStake);
    Task<LobbyStats> GetStatsAsync();
}

/// <summary>
/// Statistics for the game lobby
/// </summary>
public class LobbyStats
{
    public int TotalGames { get; set; }
    public decimal TotalStaked { get; set; }
    public decimal MinStake { get; set; }
    public decimal MaxStake { get; set; }
    public int GamesLast24Hours { get; set; }
}
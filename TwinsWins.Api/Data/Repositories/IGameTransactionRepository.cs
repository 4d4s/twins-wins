using TwinsWins.Api.Data.Models;

namespace TwinsWins.Api.Data.Repositories;

/// <summary>
/// Repository interface for GameTransaction operations
/// </summary>
public interface IGameTransactionRepository
{
    Task<GameTransactionEntity?> GetByIdAsync(long id);
    Task<GameTransactionEntity> CreateAsync(GameTransactionEntity transaction);
    Task<GameTransactionEntity> UpdateAsync(GameTransactionEntity transaction);
    Task DeleteAsync(long id);
    Task SetOpponentAsync(long gameId, long opponentId);
    Task<List<GameTransactionEntity>> GetByUserIdAsync(long userId);
    Task<List<GameTransactionEntity>> GetCompletedGamesAsync(int page = 1, int pageSize = 20);
}
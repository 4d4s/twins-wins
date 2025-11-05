using Microsoft.EntityFrameworkCore;
using TwinsWins.Api.Data.Models;

namespace TwinsWins.Api.Data.Repositories;

/// <summary>
/// Repository implementation for GameTransaction operations
/// </summary>
public class GameTransactionRepository : IGameTransactionRepository
{
    private readonly DatabaseContext _context;

    public GameTransactionRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<GameTransactionEntity?> GetByIdAsync(long id)
    {
        return await _context.GameTransactions
            .Include(g => g.Owner)
            .Include(g => g.Opponent)
            .Include(g => g.Winner)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<GameTransactionEntity> CreateAsync(GameTransactionEntity transaction)
    {
        _context.GameTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<GameTransactionEntity> UpdateAsync(GameTransactionEntity transaction)
    {
        _context.GameTransactions.Update(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task DeleteAsync(long id)
    {
        var transaction = await _context.GameTransactions.FindAsync(id);
        if (transaction != null)
        {
            _context.GameTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SetOpponentAsync(long gameId, long opponentId)
    {
        var transaction = await _context.GameTransactions.FindAsync(gameId);
        if (transaction != null)
        {
            transaction.OpponentId = opponentId;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<GameTransactionEntity>> GetByUserIdAsync(long userId)
    {
        return await _context.GameTransactions
            .Include(g => g.Owner)
            .Include(g => g.Opponent)
            .Include(g => g.Winner)
            .Where(g => g.OwnerId == userId || g.OpponentId == userId)
            .OrderByDescending(g => g.Created)
            .ToListAsync();
    }

    public async Task<List<GameTransactionEntity>> GetCompletedGamesAsync(int page = 1, int pageSize = 20)
    {
        return await _context.GameTransactions
            .Include(g => g.Owner)
            .Include(g => g.Opponent)
            .Include(g => g.Winner)
            .Where(g => g.Completed != null)
            .OrderByDescending(g => g.Completed)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
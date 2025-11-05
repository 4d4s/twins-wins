using Microsoft.EntityFrameworkCore;
using TwinsWins.Api.Data.Models;

namespace TwinsWins.Api.Data.Repositories;

/// <summary>
/// Repository implementation for GameLobby operations
/// </summary>
public class GameLobbyRepository : IGameLobbyRepository
{
    private readonly DatabaseContext _context;

    public GameLobbyRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<GameLobbyEntity?> GetByIdAsync(long id)
    {
        return await _context.GameLobbies
            .Include(g => g.Owner)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<GameLobbyEntity> CreateAsync(GameLobbyEntity lobby)
    {
        _context.GameLobbies.Add(lobby);
        await _context.SaveChangesAsync();
        return lobby;
    }

    public async Task<GameLobbyEntity> UpdateAsync(GameLobbyEntity lobby)
    {
        _context.GameLobbies.Update(lobby);
        await _context.SaveChangesAsync();
        return lobby;
    }

    public async Task DeleteAsync(long id)
    {
        var lobby = await _context.GameLobbies.FindAsync(id);
        if (lobby != null)
        {
            _context.GameLobbies.Remove(lobby);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<GameLobbyEntity>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        return await _context.GameLobbies
            .Include(g => g.Owner)
            .OrderByDescending(g => g.Created)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<GameLobbyEntity>> GetByOwnerIdAsync(long ownerId)
    {
        return await _context.GameLobbies
            .Include(g => g.Owner)
            .Where(g => g.OwnerId == ownerId)
            .OrderByDescending(g => g.Created)
            .ToListAsync();
    }

    public async Task<List<GameLobbyEntity>> GetByStakeRangeAsync(decimal minStake, decimal? maxStake)
    {
        var query = _context.GameLobbies
            .Include(g => g.Owner)
            .Where(g => g.Stake >= minStake);

        if (maxStake.HasValue)
        {
            query = query.Where(g => g.Stake <= maxStake.Value);
        }

        return await query
            .OrderByDescending(g => g.Created)
            .ToListAsync();
    }

    public async Task<LobbyStats> GetStatsAsync()
    {
        var now = DateTime.UtcNow;
        var yesterday = now.AddHours(-24);

        var games = await _context.GameLobbies.ToListAsync();

        if (!games.Any())
        {
            return new LobbyStats
            {
                TotalGames = 0,
                TotalStaked = 0,
                MinStake = 0,
                MaxStake = 0,
                GamesLast24Hours = 0
            };
        }

        return new LobbyStats
        {
            TotalGames = games.Count,
            TotalStaked = games.Sum(g => g.Stake),
            MinStake = games.Min(g => g.Stake),
            MaxStake = games.Max(g => g.Stake),
            GamesLast24Hours = games.Count(g => g.Created >= yesterday)
        };
    }
}
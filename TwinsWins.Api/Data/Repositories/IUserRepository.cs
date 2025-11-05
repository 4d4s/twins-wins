using TwinsWins.Api.Data.Models;

namespace TwinsWins.Api.Data.Repositories;

/// <summary>
/// Repository interface for User operations
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByWalletAddressAsync(string walletAddress);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(long id);
    Task<List<User>> GetAllAsync(int page = 1, int pageSize = 100);
}
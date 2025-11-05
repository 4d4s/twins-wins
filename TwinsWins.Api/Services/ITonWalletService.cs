namespace TwinsWins.Api.Services;

/// <summary>
/// Service interface for TON blockchain wallet operations
/// </summary>
public interface ITonWalletService
{
    /// <summary>
    /// Validates a TON wallet address format
    /// </summary>
    /// <param name="address">Wallet address to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool IsValidAddress(string address);

    /// <summary>
    /// Sends prize to the winner's wallet
    /// </summary>
    /// <param name="recipientAddress">Winner's wallet address</param>
    /// <param name="amount">Prize amount in TON</param>
    /// <returns>Transaction hash</returns>
    Task<string> SendPrizeAsync(string recipientAddress, decimal amount);

    /// <summary>
    /// Gets the wallet balance
    /// </summary>
    /// <param name="address">Wallet address</param>
    /// <returns>Balance in TON</returns>
    Task<decimal> GetBalanceAsync(string address);

    /// <summary>
    /// Verifies a transaction was completed
    /// </summary>
    /// <param name="transactionHash">Transaction hash to verify</param>
    /// <returns>True if transaction is confirmed</returns>
    Task<bool> VerifyTransactionAsync(string transactionHash);
}
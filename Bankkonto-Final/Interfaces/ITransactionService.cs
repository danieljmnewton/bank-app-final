namespace BankAccount_Final.Interfaces;

public interface ITransactionService
{
    /// <summary>
    /// Adds and persists a transaction record.
    /// </summary>
    Task AddAsync(Transaction record);

    /// <summary>
    /// Gets all transactions ordered by most recent first.
    /// </summary>
    Task<IReadOnlyList<Transaction>> GetAllAsync();

    /// <summary>
    /// Gets transactions for the specified account.
    /// </summary>
    Task<IReadOnlyList<Transaction>> GetByAccountAsync(Guid accountId);
}

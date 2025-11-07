namespace Bankkonto_Final.Interfaces;

public interface ITransactionService
{
    Task AddAsync(Transaction record);
    Task<IReadOnlyList<Transaction>> GetAllAsync();
    Task<IReadOnlyList<Transaction>> GetByAccountAsync(Guid accountId);
}

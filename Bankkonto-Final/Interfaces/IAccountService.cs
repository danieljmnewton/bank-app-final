namespace Bankkonto_Final.Interfaces

{
    public interface IAccountService
    {
        Task<IBankAccount> CreateAccount(string name, AccountType accountType, CurrencyType currency, decimal initialBalance);
        Task<List<IBankAccount>> GetAccounts();
        Task<IBankAccount?> GetAccountById(Guid id);
        Task<IBankAccount?> GetAccountByName(string name, AccountType accountType);
        Task DepositAsync(Guid accountId, decimal amount);
        Task WithdrawAsync(Guid accountId, decimal amount);
        Task TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount);
    }
}
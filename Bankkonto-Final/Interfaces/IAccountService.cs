namespace Bankkonto_Final.Interfaces

{
    public interface IAccountService
    {
        Task<Bankkonto> CreateAccount(string name, AccountType accountType, CurrencyType currency, decimal initialBalance);
        Task<List<Bankkonto>> GetAccounts();
        Task<Bankkonto?> GetAccountById(Guid id);
        Task<Bankkonto?> GetAccountByName(string name, AccountType accountType);
        Task DepositAsync(Guid accountId, decimal amount);
        Task WithdrawAsync(Guid accountId, decimal amount);
        Task TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount);
    }
}
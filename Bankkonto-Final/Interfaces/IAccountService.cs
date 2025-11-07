namespace Bankkonto_Final.Interfaces

{
    public interface IAccountService
    {
        Task<BankAccount> CreateAccount(string name, AccountType accountType, CurrencyType currency, decimal initialBalance);
        Task<List<BankAccount>> GetAccounts();
        Task<BankAccount?> GetAccountById(Guid id);
        Task<BankAccount?> GetAccountByName(string name, AccountType accountType);
        Task DepositAsync(Guid accountId, decimal amount);
        Task WithdrawAsync(Guid accountId, decimal amount);
        Task TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount);
    }
}

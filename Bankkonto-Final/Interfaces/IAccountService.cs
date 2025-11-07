namespace BankAccount_Final.Interfaces

{
    public interface IAccountService
    {
        /// <summary>
        /// Creates a new account.
        /// </summary>
        Task<BankAccount> CreateAccount(string name, AccountType accountType, CurrencyType currency, decimal initialBalance);

        /// <summary>
        /// Gets all accounts.
        /// </summary>
        Task<List<BankAccount>> GetAccounts();

        /// <summary>
        /// Gets an account by its ID.
        /// </summary>
        Task<BankAccount?> GetAccountById(Guid id);

        /// <summary>
        /// Gets an account by its name and type.
        /// </summary>
        Task<BankAccount?> GetAccountByName(string name, AccountType accountType);

        /// <summary>
        /// Deposits money into the specified account.
        /// </summary>
        Task DepositAsync(Guid accountId, decimal amount);

        /// <summary>
        /// Withdraws money from the specified account.
        /// </summary>
        Task WithdrawAsync(Guid accountId, decimal amount);

        /// <summary>
        /// Transfers money between two accounts.
        /// </summary>
        Task TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount);

        /// <summary>
        /// Exports accounts as JSON.
        /// </summary>
        Task<string> ExportJsonAsync();

        /// <summary>
        /// Imports accounts from JSON, replacing or merging.
        /// </summary>
        Task<List<string>> ImportJsonAsync(string json, bool replaceExisting = false);
    }
}

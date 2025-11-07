using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankAccount_Final.Services

{
    public class AccountService : IAccountService
    {
        private const string StorageKey = "bankapp.accounts";
        private readonly List<BankAccount> _accounts;
        private readonly IStorageService _storageService;
        private bool isLoaded;

        /// <summary>
        /// JSON options for export/import
        /// </summary>
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        /// <summary>
        /// Initializes a new instance of <see cref="AccountService"/>.
        /// </summary>
        /// <param name="storageService">Storage service used for persisting data.</param>
        public AccountService(IStorageService storageService)
        {
            _storageService = storageService;
            _accounts = new List<BankAccount>();
        }

        /// <summary>
        /// Loads accounts from storage on first use.
        /// </summary>
        private async Task IsInitialized()
        {
            if (isLoaded) return;
            var fromStorage = await _storageService.GetItemAsync<List<BankAccount>>(StorageKey);
            _accounts.Clear();
            if (fromStorage is { Count: > 0 })
                _accounts.AddRange(fromStorage);
            isLoaded = true;
            Console.WriteLine($"[AccountService] Initialized. Loaded {_accounts.Count} account(s) from storage.");
        }

        /// <summary>
        /// Saves the current account list to storage.
        /// </summary>
        private Task SaveAsync()
        {
            var list = _accounts.ToList();
            Console.WriteLine($"[AccountService] Saving {list.Count} account(s) to storage.");
            return _storageService.SetItemAsync(StorageKey, list);
        }

        /// <summary>
        /// Creates a new account and persists it.
        /// </summary>
        /// <param name="name">Account name.</param>
        /// <param name="accountType">Account type.</param>
        /// <param name="currency">Account currency.</param>
        /// <param name="initialBalance">Initial balance (non-negative).</param>
        /// <returns>The created account.</returns>
        /// <exception cref="ArgumentException">If name, accountType or currency are invalid, or balance is negative.</exception>
        /// <exception cref="InvalidOperationException">If a duplicate account exists.</exception>
        public async Task<BankAccount> CreateAccount(string name, AccountType accountType, CurrencyType currency, decimal initialBalance)
        {
            await IsInitialized();
            Console.WriteLine($"[AccountService] CreateAccount requested. Name='{name}', Type={accountType}, Currency={currency}, InitialBalance={initialBalance}.");
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("[AccountService] CreateAccount failed: Account name is required.");
                throw new ArgumentException("Account name is required.", nameof(name));
            }
            if (accountType == AccountType.None)
            {
                Console.WriteLine("[AccountService] CreateAccount failed: Select an account type.");
                throw new ArgumentException("Select an account type.", nameof(accountType));
            }
            if (currency == CurrencyType.None)
            {
                Console.WriteLine("[AccountService] CreateAccount failed: Select a currency.");
                throw new ArgumentException("Select a currency.", nameof(currency));
            }
            if (initialBalance < 0)
            {
                Console.WriteLine("[AccountService] CreateAccount failed: Initial balance cannot be negative.");
                throw new ArgumentException("Initial balance cannot be negative.", nameof(initialBalance));
            }

            var exists = _accounts.Any(a =>
                a.AccountType == accountType && string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
            if (exists)
            {
                Console.WriteLine("[AccountService] CreateAccount failed: Duplicate account.");
                throw new InvalidOperationException("An account with the same name and type already exists.");
            }

            var account = new BankAccount(name, accountType, currency, initialBalance);
            _accounts.Add(account);
            await SaveAsync();
            Console.WriteLine($"[AccountService] Account created. Id={account.Id}, Name='{account.Name}', Type={account.AccountType}, Currency={account.Currency}, Balance={account.Balance}.");
            return account;
        }

        /// <summary>
        /// Gets all accounts.
        /// </summary>
        /// <returns>List of all accounts.</returns>
        public async Task<List<BankAccount>> GetAccounts()
        {
            await IsInitialized();
            Console.WriteLine($"[AccountService] GetAccounts returned {_accounts.Count} account(s).");
            return _accounts.ToList();
        }

        /// <summary>
        /// Gets an account by its unique identifier.
        /// </summary>
        /// <param name="id">The account identifier.</param>
        /// <returns>The account if found; otherwise null.</returns>
        public async Task<BankAccount?> GetAccountById(Guid id)
        {
            await IsInitialized();
            Console.WriteLine($"[AccountService] GetAccountById requested. Id={id}.");
            return _accounts.FirstOrDefault(a => a.Id == id);
        }

        /// <summary>
        /// Gets an account by its name and type.
        /// </summary>
        /// <param name="name">Account name.</param>
        /// <param name="accountType">Account type.</param>
        /// <returns>The account if found; otherwise null.</returns>
        public async Task<BankAccount?> GetAccountByName(string name, AccountType accountType)
        {
            await IsInitialized();
            Console.WriteLine($"[AccountService] GetAccountByName requested. Name='{name}', Type={accountType}.");
            return _accounts.FirstOrDefault(a => a.AccountType == accountType && string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Deposits an amount into the specified account.
        /// </summary>
        /// <param name="accountId">Target account ID.</param>
        /// <param name="amount">Amount to deposit (must be positive).</param>
        /// <exception cref="InvalidOperationException">If the account is not found.</exception>
        public async Task DepositAsync(Guid accountId, decimal amount)
        {
            await IsInitialized();
            Console.WriteLine($"[AccountService] Deposit requested. AccountId={accountId}, Amount={amount}.");
            var account = _accounts.FirstOrDefault(a => a.Id == accountId);
            if (account is null)
            {
                Console.WriteLine("[AccountService] Deposit failed: Account not found.");
                throw new InvalidOperationException("Account not found.");
            }
            account.Deposit(amount);
            Console.WriteLine($"[AccountService] Deposit completed. AccountId={accountId}, NewBalance={account.Balance}.");
            await SaveAsync();
        }

        /// <summary>
        /// Withdraws an amount from the specified account.
        /// </summary>
        /// <param name="accountId">Source account ID.</param>
        /// <param name="amount">Amount to withdraw (must be positive and not exceed balance).</param>
        /// <exception cref="InvalidOperationException">If the account is not found.</exception>
        public async Task WithdrawAsync(Guid accountId, decimal amount)
        {
            await IsInitialized();
            Console.WriteLine($"[AccountService] Withdraw requested. AccountId={accountId}, Amount={amount}.");
            var account = _accounts.FirstOrDefault(a => a.Id == accountId);
            if (account is null)
            {
                Console.WriteLine("[AccountService] Withdraw failed: Account not found.");
                throw new InvalidOperationException("Account not found.");
            }
            account.Withdraw(amount);
            Console.WriteLine($"[AccountService] Withdraw completed. AccountId={accountId}, NewBalance={account.Balance}.");
            await SaveAsync();
        }

        /// <summary>
        /// Transfers an amount between two accounts.
        /// </summary>
        /// <param name="fromAccountId">Source account ID.</param>
        /// <param name="toAccountId">Destination account ID.</param>
        /// <param name="amount">Amount to transfer.</param>
        /// <exception cref="ArgumentException">If source and destination are the same account.</exception>
        /// <exception cref="InvalidOperationException">If accounts are missing or currencies differ.</exception>
        public async Task TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            await IsInitialized();

            if (fromAccountId == toAccountId)
            {
                Console.WriteLine("[AccountService] Transfer failed: From and To accounts must be different.");
                throw new ArgumentException("From and To accounts must be different.");
            }

            Console.WriteLine($"[AccountService] Transfer requested. From={fromAccountId}, To={toAccountId}, Amount={amount}.");
            var from = _accounts.FirstOrDefault(a => a.Id == fromAccountId);
            if (from is null)
            {
                Console.WriteLine("[AccountService] Transfer failed: From account not found.");
                throw new InvalidOperationException("From account not found.");
            }
            var to = _accounts.FirstOrDefault(a => a.Id == toAccountId);
            if (to is null)
            {
                Console.WriteLine("[AccountService] Transfer failed: To account not found.");
                throw new InvalidOperationException("To account not found.");
            }

            if (from.Currency != to.Currency)
            {
                Console.WriteLine("[AccountService] Transfer failed: Transfers between different currencies are not supported.");
                throw new InvalidOperationException("Transfers between different currencies are not supported.");
            }

            var fromBefore = from.Balance;
            var toBefore = to.Balance;
            from.Withdraw(amount);
            to.Deposit(amount);
            Console.WriteLine($"[AccountService] Transfer completed. From={fromAccountId} {fromBefore}->{from.Balance}, To={toAccountId} {toBefore}->{to.Balance}, Amount={amount}.");

            await SaveAsync();
        }

        /// <summary>
        /// Exports all accounts as a JSON string.
        /// </summary>
        public async Task<string> ExportJsonAsync()
        {
            await IsInitialized();
            return JsonSerializer.Serialize(_accounts, _jsonOptions);
        }

        /// <summary>
        /// Imports accounts from a JSON string, replacing or merging with existing data.
        /// </summary>
        /// <param name="json">JSON content to import.</param>
        /// <param name="replaceExisting">If true, replaces existing accounts; otherwise merges.</param>
        /// <returns>A list of error messages, or empty on success.</returns>
        public async Task<List<string>> ImportJsonAsync(string json, bool replaceExisting = false)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(json))
            {
                errors.Add("Tom JSON.");
                return errors;
            }

            List<BankAccount>? incoming;
            try
            {
                incoming = JsonSerializer.Deserialize<List<BankAccount>>(json, _jsonOptions);
            }
            catch
            {
                errors.Add("Ogiltig JSON.");
                return errors;
            }

            if (incoming is null || incoming.Count == 0)
            {
                errors.Add("Ingen data.");
                return errors;
            }

            await IsInitialized();

            if (replaceExisting)
            {
                _accounts.Clear();
                _accounts.AddRange(incoming);
            }
            else
            {
                var existing = _accounts.Select(a => a.Id).ToHashSet();
                foreach (var a in incoming)
                {
                    if (!existing.Contains(a.Id))
                        _accounts.Add(a);
                }
            }

            await SaveAsync();
            return errors;
        }
    }
}

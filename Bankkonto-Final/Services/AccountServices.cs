namespace Bankkonto_Final.Services

{
    public class AccountService : IAccountService
    {
        private const string StorageKey = "bankapp.accounts";
        private readonly List<BankAccount> _accounts;
        private readonly IStorageService _storageService;
        private bool isLoaded;

        public AccountService(IStorageService storageService)
        {
            _storageService = storageService;
            _accounts = new List<BankAccount>();
        }

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

        private Task SaveAsync()
        {
            var list = _accounts.ToList();
            Console.WriteLine($"[AccountService] Saving {list.Count} account(s) to storage.");
            return _storageService.SetItemAsync(StorageKey, list);
        }

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

        public async Task<List<BankAccount>> GetAccounts()
        {
            await IsInitialized();
            Console.WriteLine($"[AccountService] GetAccounts returned {_accounts.Count} account(s).");
            return _accounts.ToList();
        }

        public async Task<BankAccount?> GetAccountById(Guid id)
        {
            await IsInitialized();
            Console.WriteLine($"[AccountService] GetAccountById requested. Id={id}.");
            return _accounts.FirstOrDefault(a => a.Id == id);
        }

        public async Task<BankAccount?> GetAccountByName(string name, AccountType accountType)
        {
            await IsInitialized();
            Console.WriteLine($"[AccountService] GetAccountByName requested. Name='{name}', Type={accountType}.");
            return _accounts.FirstOrDefault(a => a.AccountType == accountType && string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
        }

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
    }
}

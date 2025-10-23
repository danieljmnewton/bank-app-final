namespace Bankkonto_Final.Services

{
    public class AccountService : IAccountService
    {
        private const string StorageKey = "bankapp.accounts";
        private readonly List<IBankAccount> _accounts;
        private readonly IStorageService _storageService;
        private bool isLoaded;

        public AccountService(IStorageService storageService)
        {
            _storageService = storageService;
            _accounts = new List<IBankAccount>();
        }

        private async Task IsInitialized()
        {
            if (isLoaded) return;
            var fromStorage = await _storageService.GetItemAsync<List<IBankAccount>>(StorageKey);
            _accounts.Clear();
            if (fromStorage is { Count: > 0 })
                _accounts.AddRange(fromStorage);
            isLoaded = true;
            Console.WriteLine($"[AccountService] Initialized. Loaded {_accounts.Count} account(s) from storage.");
        }

        private Task SaveAsync()
        {
            var concrete = _accounts.Select(a => (Bankkonto)a).ToList();
            Console.WriteLine($"[AccountService] Saving {concrete.Count} account(s) to storage.");
            return _storageService.SetItemAsync(StorageKey, concrete);
        }

        public async Task<IBankAccount> CreateAccount(string name, AccountType accountType, CurrencyType currency, decimal initialBalance)
        {
            await IsInitialized();
            Console.WriteLine($"[AccountService] CreateAccount requested. Name='{name}', Type={accountType}, Currency={currency}, InitialBalance={initialBalance}.");
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("[AccountService] CreateAccount failed: Kontonamn krävs.");
                throw new ArgumentException("Kontonamn krävs.", nameof(name));
            }
            if (accountType == AccountType.None)
            {
                Console.WriteLine("[AccountService] CreateAccount failed: Välj kontotyp.");
                throw new ArgumentException("Välj kontotyp.", nameof(accountType));
            }
            if (currency == CurrencyType.None)
            {
                Console.WriteLine("[AccountService] CreateAccount failed: Välj valuta.");
                throw new ArgumentException("Välj valuta.", nameof(currency));
            }
            if (initialBalance < 0)
            {
                Console.WriteLine("[AccountService] CreateAccount failed: Startsaldo kan inte vara negativt.");
                throw new ArgumentException("Startsaldo kan inte vara negativt.", nameof(initialBalance));
            }

            var exists = _accounts.Any(a =>
                a.AccountType == accountType && string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
            if (exists)
            {
                Console.WriteLine("[AccountService] CreateAccount failed: Duplicate account.");
                throw new InvalidOperationException("Ett konto med samma namn och kontotyp finns redan.");
            }

            var account = new Bankkonto(name, accountType, currency, initialBalance);
            _accounts.Add(account);
            await SaveAsync();
            Console.WriteLine($"[AccountService] Account created. Id={account.Id}, Name='{account.Name}', Type={account.AccountType}, Currency={account.Currency}, Balance={account.Balance}.");
            return account;
        }

        public async Task<List<IBankAccount>> GetAccounts()
        {
            await IsInitialized();
            Console.WriteLine($"[AccountService] GetAccounts returned {_accounts.Count} account(s).");
            return _accounts.Cast<IBankAccount>().ToList();
        }

        public async Task<IBankAccount?> GetAccountById(Guid id)
        {
            await IsInitialized();
            Console.WriteLine($"[AccountService] GetAccountById requested. Id={id}.");
            return _accounts.FirstOrDefault(a => a.Id == id);
        }

        public async Task<IBankAccount?> GetAccountByName(string name, AccountType accountType)
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
                Console.WriteLine("[AccountService] Deposit failed: Konto hittades inte.");
                throw new InvalidOperationException("Konto hittades inte.");
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
                Console.WriteLine("[AccountService] Withdraw failed: Konto hittades inte.");
                throw new InvalidOperationException("Konto hittades inte.");
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
                Console.WriteLine("[AccountService] Transfer failed: Från- och till-konto måste vara olika.");
                throw new ArgumentException("Från- och till-konto måste vara olika.");
            }

            Console.WriteLine($"[AccountService] Transfer requested. From={fromAccountId}, To={toAccountId}, Amount={amount}.");
            var from = _accounts.FirstOrDefault(a => a.Id == fromAccountId);
            if (from is null)
            {
                Console.WriteLine("[AccountService] Transfer failed: Från-kontot hittades inte.");
                throw new InvalidOperationException("Från-kontot hittades inte.");
            }
            var to = _accounts.FirstOrDefault(a => a.Id == toAccountId);
            if (to is null)
            {
                Console.WriteLine("[AccountService] Transfer failed: Till-kontot hittades inte.");
                throw new InvalidOperationException("Till-kontot hittades inte.");
            }

            if (from.Currency != to.Currency)
            {
                Console.WriteLine("[AccountService] Transfer failed: Överföring mellan olika valutor stöds inte.");
                throw new InvalidOperationException("Överföring mellan olika valutor stöds inte.");
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
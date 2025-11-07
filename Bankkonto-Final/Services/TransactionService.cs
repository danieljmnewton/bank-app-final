namespace BankAccount_Final.Services

{
    public class TransactionService : ITransactionService
    {
        private const string StorageKey = "bankapp_final.transactions";
        private readonly IStorageService _storageService;
        private readonly List<Transaction> _records = new();
        private bool _loaded;

        /// <summary>
        /// Initializes a new instance of <see cref="TransactionService"/>.
        /// </summary>
        /// <param name="storageService">Storage service used to persist transactions.</param>
        public TransactionService(IStorageService storageService)
        {
            _storageService = storageService;
        }

        /// <summary>
        /// Ensures transactions are loaded from storage into memory.
        /// </summary>
        private async Task EnsureLoaded()
        {
            if (_loaded) return;
            var stored = await _storageService.GetItemAsync<List<Transaction>>(StorageKey) ?? new List<Transaction>();
            _records.Clear();
            _records.AddRange(stored);
            _loaded = true;
            Console.WriteLine($"[TransactionService] Initialized. Loaded {_records.Count} transaction(s) from storage.");
        }

        /// <summary>
        /// Persists current transactions to storage.
        /// </summary>
        private Task SaveAsync() => _storageService.SetItemAsync(StorageKey, _records);

        /// <summary>
        /// Adds a transaction record and persists it.
        /// </summary>
        /// <param name="record">The transaction to add.</param>
        public async Task AddAsync(Transaction record)
        {
            await EnsureLoaded();
            Console.WriteLine($"[TransactionService] Adding transaction. Id={record.Id}, Type={record.Type}, Amount={record.Amount}, From={record.FromAccountId}, To={record.ToAccountId}, Currency={record.Currency}.");
            _records.Add(record);
            await SaveAsync();
            Console.WriteLine($"[TransactionService] Transaction stored. Total records: {_records.Count}.");
        }

        /// <summary>
        /// Gets all transactions, ordered by most recent first.
        /// </summary>
        /// <returns>List of transactions.</returns>
        public async Task<IReadOnlyList<Transaction>> GetAllAsync()
        {
            await EnsureLoaded();
            var result = _records
                .OrderByDescending(r => r.Timestamp)
                .ToList();
            Console.WriteLine($"[TransactionService] GetAllAsync returned {result.Count} transaction(s).");
            return result;
        }

        /// <summary>
        /// Gets transactions related to a specific account.
        /// </summary>
        /// <param name="accountId">The account ID.</param>
        /// <returns>List of matching transactions.</returns>
        public async Task<IReadOnlyList<Transaction>> GetByAccountAsync(Guid accountId)
        {
            await EnsureLoaded();
            var result = _records
                .Where(r =>
                    (r.FromAccountId.HasValue && r.FromAccountId.Value == accountId) ||
                    (r.ToAccountId.HasValue && r.ToAccountId.Value == accountId))
                .OrderByDescending(r => r.Timestamp)
                .ToList();
            Console.WriteLine($"[TransactionService] GetByAccountAsync for {accountId} returned {result.Count} transaction(s).");
            return result;
        }
    }
}

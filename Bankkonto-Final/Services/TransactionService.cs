namespace Bankkonto_Final.Services
{
    public class TransactionService : ITransactionService
    {
        private const string StorageKey = "bankapp_final.transactions";
        private readonly IStorageService _storageService;
        private readonly List<Transaction> _records = new();
        private bool _loaded;

        public TransactionService(IStorageService storageService)
        {
            _storageService = storageService;
        }

        private async Task EnsureLoaded()
        {
            if (_loaded) return;
            var stored = await _storageService.GetItemAsync<List<Transaction>>(StorageKey) ?? new List<Transaction>();
            _records.Clear();
            _records.AddRange(stored);
            _loaded = true;
            Console.WriteLine($"[TransactionService] Initialized. Loaded {_records.Count} transaction(s) from storage.");
        }

        private Task SaveAsync() => _storageService.SetItemAsync(StorageKey, _records);

        public async Task AddAsync(Transaction record)
        {
            await EnsureLoaded();
            Console.WriteLine($"[TransactionService] Adding transaction. Id={record.Id}, Type={record.Type}, Amount={record.Amount}, From={record.FromAccountId}, To={record.ToAccountId}, Currency={record.Currency}.");
            _records.Add(record);
            await SaveAsync();
            Console.WriteLine($"[TransactionService] Transaction stored. Total records: {_records.Count}.");
        }

        public async Task<IReadOnlyList<Transaction>> GetAllAsync()
        {
            await EnsureLoaded();
            var result = _records
                .OrderByDescending(r => r.Timestamp)
                .ToList();
            Console.WriteLine($"[TransactionService] GetAllAsync returned {result.Count} transaction(s).");
            return result;
        }

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
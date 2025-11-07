namespace Bankkonto_Final.Services;

public class PinLockService : IPinLockService
{
    private const string StorageKey = "IsUnlocked";
    private readonly IStorageService _storage;

    public bool IsUnlocked { get; private set; }
    public event Action? OnChange;

    public PinLockService(IStorageService storage)
    {
        _storage = storage;
    }

    public async Task InitializeAsync()
    {
        var stored = await _storage.GetItemAsync<bool?>(StorageKey);
        IsUnlocked = stored ?? false;
        OnChange?.Invoke();
    }

    public async Task<bool> TryUnlockAsync(string pin)
    {
        if (pin == "9867")
        {
            IsUnlocked = true;
            await _storage.SetItemAsync(StorageKey, true);
            OnChange?.Invoke();
            return true;
        }
        return false;
    }

    public async Task LockAsync()
    {
        IsUnlocked = false;
        await _storage.SetItemAsync(StorageKey, false);
        OnChange?.Invoke();
    }
}

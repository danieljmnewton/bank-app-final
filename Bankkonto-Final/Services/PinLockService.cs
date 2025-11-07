namespace BankAccount_Final.Services;

public class PinLockService : IPinLockService
{
    private const string StorageKey = "IsUnlocked";
    private readonly IStorageService _storage;

    public bool IsUnlocked { get; private set; }
    public event Action? OnChange;

    /// <summary>
    /// Initializes a new instance of <see cref="PinLockService"/>.
    /// </summary>
    /// <param name="storage">Storage service for persisting lock state.</param>
    public PinLockService(IStorageService storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// Loads the current unlocked state from storage.
    /// </summary>
    public async Task InitializeAsync()
    {
        var stored = await _storage.GetItemAsync<bool?>(StorageKey);
        IsUnlocked = stored ?? false;
        OnChange?.Invoke();
    }

    /// <summary>
    /// Attempts to unlock using the provided PIN.
    /// </summary>
    /// <param name="pin">4-digit PIN.</param>
    /// <returns>True if unlocked, otherwise false.</returns>
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

    /// <summary>
    /// Locks the app and persists the locked state.
    /// </summary>
    public async Task LockAsync()
    {
        IsUnlocked = false;
        await _storage.SetItemAsync(StorageKey, false);
        OnChange?.Invoke();
    }
}

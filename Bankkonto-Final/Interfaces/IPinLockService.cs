namespace BankAccount_Final.Interfaces;

public interface IPinLockService
{
    /// <summary>
    /// Indicates whether the app is currently unlocked.
    /// </summary>
    bool IsUnlocked { get; }
    event Action? OnChange;
    /// <summary>
    /// Initializes the service by loading state from storage.
    /// </summary>
    Task InitializeAsync();
    /// <summary>
    /// Tries to unlock with the provided PIN.
    /// </summary>
    Task<bool> TryUnlockAsync(string pin);
    /// <summary>
    /// Locks and persists the locked state.
    /// </summary>
    Task LockAsync();
}

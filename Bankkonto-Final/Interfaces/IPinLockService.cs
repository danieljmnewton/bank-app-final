namespace Bankkonto_Final.Interfaces;

public interface IPinLockService
{
    bool IsUnlocked { get; }
    event Action? OnChange;
    Task InitializeAsync();
    Task<bool> TryUnlockAsync(string pin);
    Task LockAsync();
}

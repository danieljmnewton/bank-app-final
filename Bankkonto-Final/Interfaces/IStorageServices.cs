namespace BankAccount_Final.Interfaces;

public interface IStorageService
{
    /// <summary>
    /// Saves a value to storage under a key.
    /// </summary>
    Task SetItemAsync<T>(string key, T value);
    /// <summary>
    /// Retrieves a value from storage by key.
    /// </summary>
    Task<T?> GetItemAsync<T>(string key);
}

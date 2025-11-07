namespace Bankkonto_Final.Interfaces;

public interface IStorageService
{
    Task SetItemAsync<T>(string key, T value);
    Task<T?> GetItemAsync<T>(string key);
}

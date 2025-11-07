using Microsoft.JSInterop;
using System.Text.Json;

namespace BankAccount_Final.Services

{
    public class StorageService : IStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Initializes a new instance of <see cref="StorageService"/>.
        /// </summary>
        /// <param name="jsRuntime">JS runtime used to access browser storage.</param>
        public StorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Stores a value under a given key in localStorage.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="key">Storage key.</param>
        /// <param name="value">Value to store.</param>
        public async Task SetItemAsync<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            Console.WriteLine($"[StorageService] SetItem key='{key}', length={json?.Length ?? 0}.");
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }

        /// <summary>
        /// Retrieves a value by key from localStorage.
        /// </summary>
        /// <typeparam name="T">Expected type of the value.</typeparam>
        /// <param name="key">Storage key.</param>
        /// <returns>Deserialized value or default if not found.</returns>
        public async Task<T?> GetItemAsync<T>(string key)
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine($"[StorageService] GetItem key='{key}' not found.");
                return default;
            }
            Console.WriteLine($"[StorageService] GetItem key='{key}' hit, length={json.Length}.");
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
    }
}

using Microsoft.JSInterop;
using System.Text.Json;

namespace Bankkonto_Final.Services

{
    public class StorageService : IStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public StorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SetItemAsync<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            Console.WriteLine($"[StorageService] SetItem key='{key}', length={json?.Length ?? 0}.");
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }

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

using Microsoft.JSInterop;

namespace PostHive.Services
{
    /// <summary>
    /// Service for interacting with the browser's localStorage using JavaScript Interop.
    /// </summary>
    public class LocalStorageService
    {
        private readonly IJSRuntime jsRuntime;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalStorageService"/> class.
        /// </summary>
        /// <param name="jsRuntime">The IJSRuntime instance used to interact with JavaScript.</param>
        public LocalStorageService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Sets an item in the browser's localStorage.
        /// </summary>
        /// <param name="key">The key to store the item under.</param>
        /// <param name="value">The value to store.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SetItemAsync(string key, string value)
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }

        /// <summary>
        /// Retrieves an item from the browser's localStorage.
        /// </summary>
        /// <param name="key">The key of the item to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the value stored under the specified key, or null if not found.</returns>
        public async Task<string> GetItemAsync(string key)
        {
            return await jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
        }

        /// <summary>
        /// Removes an item from the browser's localStorage.
        /// </summary>
        /// <param name="key">The key of the item to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveItemAsync(string key)
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }

        /// <summary>
        /// Clears all items from the browser's localStorage.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ClearAsync()
        {
            await jsRuntime.InvokeVoidAsync("localStorage.clear");
        }
    }
}

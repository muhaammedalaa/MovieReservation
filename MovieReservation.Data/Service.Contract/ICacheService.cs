namespace MovieReservation.Data.Service.Contract
{
    
    public interface ICacheService
    {
        /// Get value from cache
        Task<T?> GetAsync<T>(string key);
        /// Set value in cache with expiration
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        /// Remove specific cache key
        Task RemoveAsync(string key);
        /// Remove all cache keys with specific prefix
        Task RemoveByPatternAsync(string pattern);
    }
}
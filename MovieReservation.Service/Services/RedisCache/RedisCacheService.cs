using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MovieReservation.Data.Service.Contract;
using StackExchange.Redis;
using System.Text.Json;


namespace MovieReservation.Service.Services.RedisCache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly IDatabase _db;

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> logger,IConfiguration configuration)
        {
            _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            int databaseNumber = 0;
            var redisSection = configuration.GetSection("Redis");
            var connectionString = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(connectionString))
            {
                if (connectionString.Contains("defaultDatabase="))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(connectionString, @"defaultDatabase=(\d+)");
                    if (match.Success)
                    {
                        databaseNumber = int.Parse(match.Groups[1].Value);
                    }
                }
            }

            // أو اقرأ من قسم Redis في الإعدادات
            if (redisSection.Exists())
            {
                databaseNumber = redisSection.GetValue<int>("Database", databaseNumber);
            }

            // ⭐ 2. أنشئ اتصال قاعدة البيانات
            _db = _connectionMultiplexer.GetDatabase(databaseNumber);

            // ⭐ 3. الآن يمكنك استخدام _db في السجلات
            _logger.LogInformation(
                "🔍 Redis Cache Service initialized. " +
                "IsConnected: {IsConnected} | " +
                "Database: {Database} | " +
                "ConnectionString: {ConnectionString}",
                IsConnected,
                _db.Database,
                string.IsNullOrEmpty(connectionString) ? "default" : connectionString);

            // ⭐ 4. اختبر الاتصال
            TestConnectionAsync().Wait();
        }
        

        public async Task<T?> GetAsync<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (!IsConnected)
            {
                _logger.LogWarning("⚠️ Redis not connected for GET operation on key: {Key}", key);
                return default;
            }

            try
            {
                _logger.LogDebug("🔍 Attempting to GET key from Redis: {Key}", key);

                var value = await _db.StringGetAsync(key);

                if (!value.HasValue)
                {
                    _logger.LogWarning(
                        "❌ Cache MISS - Key not found in Redis: {Key} | Database: {Database}",
                        key, _db.Database);

                    // DIAGNOSTIC: List all keys in Redis
                    await LogAllKeysInRedis();

                    return default;
                }

                _logger.LogInformation("✅ Cache HIT - Retrieved from Redis: {Key}", key);

                try
                {
                    var deserializedValue = JsonSerializer.Deserialize<T>(value.ToString());
                    _logger.LogDebug("✅ Successfully deserialized value for key: {Key} | Type: {Type}", key, typeof(T).Name);
                    return deserializedValue;
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "❌ Failed to deserialize JSON for key: {Key} | Type: {Type}", key, typeof(T).Name);
                    return default;
                }
            }
            catch (RedisConnectionException redisEx)
            {
                _logger.LogError(redisEx, "🔴 Redis connection error on GET for key: {Key}", key);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔴 Unexpected error retrieving from Redis cache for key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (value == null)
            {
                _logger.LogWarning("⚠️ Attempted to cache null value for key: {Key}", key);
                return;
            }

            try
            {
                if (!IsConnected)
                {
                    _logger.LogWarning("⚠️ Redis not connected for SET operation on key: {Key}", key);
                    return;
                }
                var databaseId = _db.Database; // تحقق من رقم قاعدة البيانات
                _logger.LogInformation("🔢 Using Redis database: {DatabaseId}", databaseId);
                var serializedValue = JsonSerializer.Serialize(value);
                var expirationTime = expiration ?? TimeSpan.FromMinutes(30);

                _logger.LogDebug(
                    "🔍 Attempting to SET key in Redis: {Key} | Database: {Database} | Expiration: {ExpirationMinutes} min",
                    key, _db.Database, expirationTime.TotalMinutes);

                bool setSuccess = await _db.StringSetAsync(key, serializedValue, expirationTime);

                if (setSuccess)
                {
                    _logger.LogInformation(
                        "📝 Cache SET SUCCESS - Key: {Key} | Type: {Type} | Expiration: {ExpirationMinutes} min | Size: {SizeBytes} bytes | Database: {Database}",
                        key, typeof(T).Name, expirationTime.TotalMinutes, serializedValue.Length, _db.Database);

                    // CRITICAL: Verify immediately
                    await Task.Delay(100); // Small delay to ensure write completes
                    var verification = await _db.StringGetAsync(key);

                    if (verification.HasValue)
                    {
                        _logger.LogInformation(
                            "✅ Verification SUCCESS - Key is immediately retrievable from Redis: {Key}",
                            key);
                    }
                    else
                    {
                        _logger.LogError(
                            "🔴 CRITICAL: Verification FAILED - Key CANNOT be retrieved immediately after SET: {Key} | Database: {Database}",
                            key, _db.Database);

                        // Try to list all keys
                        await LogAllKeysInRedis();
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "⚠️ Cache SET FAILED - Redis returned false for key: {Key}", key);
                }
            }
            catch (RedisConnectionException redisEx)
            {
                _logger.LogError(redisEx, "🔴 Redis connection error on SET for key: {Key}", key);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx,
                    "🔴 JSON serialization error for key: {Key} | Type: {Type}",
                    key, typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔴 Unexpected error setting Redis cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key cannot be empty", nameof(key));

            try
            {
                bool deleted = await _db.KeyDeleteAsync(key);

                if (deleted)
                {
                    _logger.LogInformation("🗑️ Cache key deleted: {Key}", key);
                }
                else
                {
                    _logger.LogDebug("ℹ️ Cache key did not exist: {Key}", key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing Redis cache: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                throw new ArgumentException("Cache pattern cannot be empty", nameof(pattern));

            try
            {
                var server = _connectionMultiplexer.GetServer(
                    _connectionMultiplexer.GetEndPoints().FirstOrDefault()!);

                var keys = server.Keys(
                    database: _db.Database,
                    pattern: $"*{pattern}*");

                var keysArray = keys.ToArray();

                if (keysArray.Length > 0)
                {
                    long deletedCount = await _db.KeyDeleteAsync(keysArray);
                    _logger.LogInformation(
                        "🗑️ Cache pattern removal - Pattern: {Pattern} | Keys found: {Found} | Keys deleted: {Deleted}",
                        pattern, keysArray.Length, deletedCount);
                }
                else
                {
                    _logger.LogDebug("ℹ️ No cache keys matching pattern: {Pattern}", pattern);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing Redis cache by pattern: {Pattern}", pattern);
            }
        }

        /// <summary>
        /// DIAGNOSTIC METHOD: List all keys in Redis database
        /// </summary>
        private async Task LogAllKeysInRedis()
        {
            try
            {
                var server = _connectionMultiplexer.GetServer(
                    _connectionMultiplexer.GetEndPoints().FirstOrDefault()!);

                var allKeys = server.Keys(database: _db.Database);
                var keysArray = allKeys.ToArray();

                if (keysArray.Length == 0)
                {
                    _logger.LogWarning("🚨 REDIS IS EMPTY - No keys found in database {Database}", _db.Database);
                }
                else
                {
                    _logger.LogInformation("📊 Redis contains {Count} keys in database {Database}:", keysArray.Length, _db.Database);
                    foreach (var k in keysArray.Take(10)) // Show first 10 keys
                    {
                        var ttl = await _db.KeyTimeToLiveAsync(k);
                        _logger.LogInformation("  - Key: {Key} | TTL: {TTL} seconds", k.ToString(), ttl?.TotalSeconds ?? -1);
                    }

                    if (keysArray.Length > 10)
                    {
                        _logger.LogInformation("  ... and {MoreCount} more keys", keysArray.Length - 10);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing Redis keys");
            }
        }

        public bool IsConnected => _connectionMultiplexer?.IsConnected ?? false;
        private async Task TestConnectionAsync()
        {
            try
            {
                var pingResult = await _db.PingAsync();
                _logger.LogInformation("🏓 Redis Ping: {PingTime} ms", pingResult.TotalMilliseconds);

                // تحقق من المفاتيح الموجودة
                var endpoints = _connectionMultiplexer.GetEndPoints();
                var server = _connectionMultiplexer.GetServer(endpoints.First());
                var keys = server.Keys(database: _db.Database, pattern: "*").Count();

                _logger.LogInformation("🔑 Found {Count} keys in database {Database}", keys, _db.Database);

                if (keys > 0)
                {
                    foreach (var key in server.Keys(database: _db.Database, pattern: "*").Take(5))
                    {
                        var ttl = await _db.KeyTimeToLiveAsync(key);
                        _logger.LogInformation("   📍 Key: {Key}, TTL: {TTL}", key, ttl);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🧪 Redis connection test failed");
            }
        }
    }
}

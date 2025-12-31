using System.Collections.Concurrent;

namespace MovieReservation.APi.Middleware
{
    /// <summary>
    /// Rate limiting middleware to prevent API abuse
    /// Tracks requests per IP address and enforces limits
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly IConfiguration _configuration;

        // Dictionary to track request counts per IP
        private static readonly ConcurrentDictionary<string, (int Count, DateTime ResetTime)> RequestCounts =
            new();

        // Default configuration values
        private int _requestsPerMinute = 100;
        private int _requestsPerHour = 1000;
        private bool _isEnabled = true;

        public RateLimitingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;

            // Read configuration
            var rateLimitConfig = _configuration.GetSection("RateLimiting");
            if (rateLimitConfig.Exists())
            {
                _requestsPerMinute = rateLimitConfig.GetValue("RequestsPerMinute", 100);
                _requestsPerHour = rateLimitConfig.GetValue("RequestsPerHour", 1000);
                _isEnabled = rateLimitConfig.GetValue("Enabled", true);
            }

            _logger.LogInformation(
                "Rate limiting configured: {RequestsPerMinute} req/min, {RequestsPerHour} req/hour, Enabled: {IsEnabled}",
                _requestsPerMinute, _requestsPerHour, _isEnabled);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip rate limiting if disabled
            if (!_isEnabled)
            {
                await _next(context);
                return;
            }

            // Get client IP address
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Skip rate limiting for webhook endpoints and health checks
            if (context.Request.Path.StartsWithSegments("/api/webhook") ||
                context.Request.Path.StartsWithSegments("/health") ||
                context.Request.Path.StartsWithSegments("/alive"))
            {
                await _next(context);
                return;
            }

            // Check rate limit
            var (isAllowed, retryAfter) = CheckRateLimit(ipAddress);

            if (!isAllowed)
            {
                _logger.LogWarning(
                    "Rate limit exceeded for IP {IpAddress}. Retry after {RetryAfter} seconds",
                    ipAddress, retryAfter);

                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = retryAfter.ToString();
                context.Response.Headers["X-RateLimit-Limit"] = _requestsPerMinute.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = "0";
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Rate limit exceeded",
                    retryAfter = retryAfter,
                    message = $"Too many requests. Please try again in {retryAfter} seconds.",
                    statusCode = 429
                });

                return;
            }

            await _next(context);
        }

        /// <summary>
        /// Check if IP has exceeded rate limit
        /// Returns (IsAllowed, RetryAfterSeconds)
        /// </summary>
        private (bool IsAllowed, int RetryAfterSeconds) CheckRateLimit(string ipAddress)
        {
            var now = DateTime.UtcNow;

            if (!RequestCounts.TryGetValue(ipAddress, out var current))
            {
                // First request from this IP
                RequestCounts.TryAdd(ipAddress, (1, now.AddMinutes(1)));
                return (true, 0);
            }

            // Reset count if minute has passed     
            if (now > current.ResetTime)
            {
                RequestCounts[ipAddress] = (1, now.AddMinutes(1));
                return (true, 0);
            }

            // Check if limit exceeded
            if (current.Count >= _requestsPerMinute)
            {
                var secondsUntilReset = (int)Math.Ceiling((current.ResetTime - now).TotalSeconds);
                return (false, secondsUntilReset);
            }

            // Increment counter
            RequestCounts[ipAddress] = (current.Count + 1, current.ResetTime);
            return (true, 0);
        }
    }
}
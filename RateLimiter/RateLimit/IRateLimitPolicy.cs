namespace RateLimiter.RateLimit
{
    public interface IRateLimitPolicy
    {
        public bool DoesThrottle(string? userName);
    }
}
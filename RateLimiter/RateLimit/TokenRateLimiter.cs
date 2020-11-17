using System;

namespace RateLimiter.RateLimit
{
    public class TokenRateLimiter : IRateLimitPolicy
    {
        public bool DoesThrottle(string? userName)
        {
            Console.WriteLine("Enter.");
            return false;
        }
    }
}
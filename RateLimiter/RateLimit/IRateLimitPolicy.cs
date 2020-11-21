using System.Threading.Tasks;

namespace RateLimiter.RateLimit
{
    public interface IRateLimitPolicy
    {
        public Task<bool> DoesThrottle(string userName);
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RateLimiter.Configuration;
using RateLimiter.Repository.Redis;

namespace RateLimiter.RateLimit
{
    internal class TokenRateLimiter : IRateLimitPolicy
    {
        private readonly RedisUserTokenRepository _repository;
        private readonly int _refillInterval;
        private readonly int _refillToken;
        private readonly int _maximumToken;

        public TokenRateLimiter(RedisUserTokenRepository repository, IConfiguration configuration)
        {
            _repository = repository;

            var options = configuration.GetSection(TokenStoreOptions.TokenStore).Get<TokenStoreOptions>();
            _refillInterval = options.RefillInterval;
            _refillToken = options.RefillToken;
            _maximumToken = options.MaximumToken;
        }

        public async Task<bool> DoesThrottle(string userName)
        {
            var current = DateTimeOffset.Now.ToUniversalTime();

            var res = await _repository.GetTokenPayload(userName);

            if (res != null)
            {
                var (token, timestamp) = res.Value;
                var nextRefillTiming = timestamp.AddSeconds(_refillInterval);

                if (token == 0 && current < nextRefillTiming) return true;

                // refill and update timestamp
                if (current >= nextRefillTiming && _maximumToken > token)
                {
                    var span = current.Subtract(timestamp);
                    var diff = (int) span.TotalSeconds / _refillInterval;
                    
                    token = token + _refillToken * diff > _maximumToken
                        ? _maximumToken
                        : token + _refillToken;

                    timestamp = current;
                }

                await _repository.PutTokenPayload(userName, token - 1, timestamp);
            }
            else
            {
                await _repository.PutTokenPayload(userName, _refillToken - 1, current);
            }

            return false;
        }
    }
}
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RateLimiter.Configuration;
using StackExchange.Redis;

namespace RateLimiter.Repository.Redis
{
    public class RedisUserTokenRepository
    {
        private readonly Lazy<ConnectionMultiplexer> _redis;
        private const string TokenHashKey = "late-limiter:token-hash";
        private const string TokenFiled = "token";
        private const string TimestampField = "ts";

        public RedisUserTokenRepository(IConfiguration configuration)
        {
            _redis = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(
                configuration.GetSection(TokenStoreOptions.TokenStore).Get<TokenStoreOptions>().ConnectionString));
        }

        public async Task<(int token, DateTimeOffset timestamp)?> GetTokenPayload(string userId)
        {
            var value = (string?) await _redis.Value.GetDatabase().HashGetAsync(TokenHashKey, userId);

            if (string.IsNullOrEmpty(value)) return null;

            using var document = JsonDocument.Parse(value);
            var root = document.RootElement;
            var token = root.GetProperty(TokenFiled).GetInt32();
            var timestamp = root.GetProperty(TimestampField).GetInt64();

            return (token, DateTimeOffset.FromUnixTimeSeconds(timestamp));

        }

        public async Task PutTokenPayload(string userId, int token, DateTimeOffset timestamp)
        {
            using var ms = new MemoryStream();
            using (var writer = new Utf8JsonWriter(ms))
            {
                writer.WriteStartObject();
                writer.WriteNumber(TokenFiled, token);
                writer.WriteNumber(TimestampField, timestamp.ToUnixTimeSeconds());
                writer.WriteEndObject();
            }

            await _redis.Value.GetDatabase().HashSetAsync(TokenHashKey, userId, Encoding.UTF8.GetString(ms.ToArray()));
        }
    }
}
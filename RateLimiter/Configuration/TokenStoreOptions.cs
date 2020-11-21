namespace RateLimiter.Configuration
{
    public class TokenStoreOptions
    {
        public const string TokenStore = "TokenStore";

        public string? ConnectionString { get; set; }
        public int RefillInterval { get; set; }
        public int RefillToken { get; set; }
        public int MaximumToken { get; set; }
    }
}
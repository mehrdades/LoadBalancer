using StackExchange.Redis;

namespace LoadBalancer.Microservice
{ 
    public class ConfigCacheService : ICacheService
    {
        private readonly IDatabase _database;

        public ConfigCacheService(IConfiguration configuration)
        {
            var redisConfiguration = configuration.GetConnectionString("RedisConnection");
            var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfiguration);
            _database = connectionMultiplexer.GetDatabase();
        }

        public string GetValue(string key)
        {
            string? cachedValue = _database.StringGet(key);
            return cachedValue ?? string.Empty;
        }

        public void SetValue(string key, string value)
        {
            _database.StringSet(key, value);
        }

    }
}
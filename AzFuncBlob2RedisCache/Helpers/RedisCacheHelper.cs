using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using StackExchange.Redis;

namespace AzFuncBlob2RedisCache.Helpers
{
    /// <summary>
    /// Helper class to write value to Azure Redis Cache
    /// </summary>
    internal class RedisCacheHelper
    {
      
        /// <summary>
        /// Instance of Redis Database
        /// </summary>
        private IDatabase redisDatabase { get; }

        /// <summary>
        /// Constructor. Returns instance of the helper class initialized with the Redis connection string.
        /// </summary>
        /// <param name="connectionString">Connection string to Redis database</param>
        public RedisCacheHelper(string connectionString)
        {
            ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
            redisDatabase = connectionMultiplexer.GetDatabase();
        }

        /// <summary>
        /// Writes the specified key-value pair to Azure Redis database.
        /// </summary>
        /// <param name="key">Key of the item</param>
        /// <param name="value">Value contained for the item</param>
        /// <returns>Awaitable Task instance for the operation</returns>
        public async Task WriteKeyValuePairAsync(string key, string value)
        {
            await redisDatabase.StringSetAsync(key, value);
        }

        /// <summary>
        /// Writes a list of specificed key-value pairs as a batch to Azure Redis database.
        /// Keys in the batch must be unique.
        /// </summary>
        /// <param name="keyValuePairs">Key Value pairs</param>
        /// <returns>Awaitable Task instance for the operation</returns>
        public async Task BatchWriteKeyValuePairAsync(IDictionary<string, string> keyValuePairs)
        {
            List<KeyValuePair<RedisKey, RedisValue>> batch = new List<KeyValuePair<RedisKey, RedisValue>>();

            keyValuePairs.ToList().ForEach((item) => {
                batch.Add(new KeyValuePair<RedisKey, RedisValue>(item.Key, item.Value));
            });

            await redisDatabase.StringSetAsync(batch.ToArray());
        }
    }
}

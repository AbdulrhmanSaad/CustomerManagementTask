using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CustomersTask4.Services.Caching
{
    public class RedisCachingService : IRedisCachingService
    {
        private readonly IDistributedCache? _cache;

        public RedisCachingService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public T? GetData<T>(string key)
        {
            var date=_cache?.GetString(key);

            if(date == null)
            {
                return default(T);
            }
            return JsonSerializer.Deserialize<T>(date);


        }

        public void SetData<T>(string key, T value)
        {
            var option = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60),
            };
            _cache?.SetString(key, JsonSerializer.Serialize(value), option);
        }
        public void RemoveData(string key)
        {
            //_cache?.Remove(key);
        }
    }
}

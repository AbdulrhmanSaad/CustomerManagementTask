namespace CustomersTask4.Services.Caching
{
    public interface IRedisCachingService
    {
        T? GetData<T>(string key);
        void SetData<T>(string key, T value);
        void RemoveData(string key);
    }
}

namespace LoadBalancer.Microservice
{
    public interface ICacheService
    {
        string GetValue(string key);
        void SetValue(string key, string value);
    }
}
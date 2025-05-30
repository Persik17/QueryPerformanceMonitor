using System.Data;

namespace QueryPerformanceMonitor.Providers
{
    public interface IQueryProvider
    {
        string ProviderName { get; }
        string OptimizeQuery(string query);
        Dictionary<string, object> GetPerformanceMetrics(IDbConnection connection);
    }
}

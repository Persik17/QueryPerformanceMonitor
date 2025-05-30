using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QueryPerformanceMonitor.Core
{
    public class QueryPerformanceMonitorService : IQueryPerformanceMonitor
    {
        private readonly QueryMonitoringOptions _options;
        private readonly ILogger<QueryPerformanceMonitorService> _logger;

        public QueryPerformanceMonitorService(
            IOptions<QueryMonitoringOptions> options,
            ILogger<QueryPerformanceMonitorService> logger = null)
        {
            _options = options.Value;
            _logger = logger;
        }

        public event Action<SlowQueryInfo> SlowQueryDetected;
        public event Action<QueryExecutionInfo> QueryExecuted;

        public void RegisterQuery(QueryExecutionInfo queryInfo)
        {
            queryInfo.Timestamp = DateTime.UtcNow;
            queryInfo.StackTrace = _options.IncludeStackTrace ? Environment.StackTrace : null;

            // Проверяем, является ли запрос медленным
            var threshold = GetThresholdForProvider(queryInfo.DatabaseProvider);
            if (queryInfo.Duration >= threshold)
            {
                queryInfo.IsSlowQuery = true;
                var slowQueryInfo = new SlowQueryInfo
                {
                    Query = queryInfo.Query,
                    Duration = queryInfo.Duration,
                    Source = queryInfo.Source,
                    DatabaseProvider = queryInfo.DatabaseProvider,
                    Timestamp = queryInfo.Timestamp,
                    ConnectionString = queryInfo.ConnectionString,
                    Parameters = queryInfo.Parameters,
                    StackTrace = queryInfo.StackTrace,
                    IsSlowQuery = true,
                    ThresholdExceeded = queryInfo.Duration - threshold
                };

                SlowQueryDetected?.Invoke(slowQueryInfo);
                _logger?.LogWarning("Slow query detected: {Duration}ms, Provider: {Provider}, Source: {Source}",
                    queryInfo.Duration.TotalMilliseconds, queryInfo.DatabaseProvider, queryInfo.Source);
            }

            QueryExecuted?.Invoke(queryInfo);
        }

        private TimeSpan GetThresholdForProvider(string provider)
        {
            return provider?.ToLower() switch
            {
                "postgresql" => _options.PostgreSqlSlowQueryThreshold,
                "sqlserver" => _options.SqlServerSlowQueryThreshold,
                _ => _options.DefaultSlowQueryThreshold
            };
        }
    }
}

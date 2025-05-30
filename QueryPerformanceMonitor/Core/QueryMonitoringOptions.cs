using System;
using System.Collections.Generic;

namespace QueryPerformanceMonitor.Core
{
    public class QueryMonitoringOptions
    {
        public TimeSpan DefaultSlowQueryThreshold { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan SqlServerSlowQueryThreshold { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan PostgreSqlSlowQueryThreshold { get; set; } = TimeSpan.FromSeconds(1);
        public bool IncludeStackTrace { get; set; } = true;
        public bool IncludeParameters { get; set; } = false;
        public int MaxQueryLength { get; set; } = 1000;
        public List<Action<SlowQueryInfo>> SlowQueryHandlers { get; set; } = new();
        public List<Action<QueryExecutionInfo>> QueryHandlers { get; set; } = new();
    }
}

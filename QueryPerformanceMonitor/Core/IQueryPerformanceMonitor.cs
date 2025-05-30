using System;

namespace QueryPerformanceMonitor.Core
{
    public interface IQueryPerformanceMonitor
    {
        void RegisterQuery(QueryExecutionInfo queryInfo);
        event Action<SlowQueryInfo> SlowQueryDetected;
        event Action<QueryExecutionInfo> QueryExecuted;
    }
}

namespace QueryPerformanceMonitor.Core
{
    public class SlowQueryInfo : QueryExecutionInfo
    {
        public TimeSpan ThresholdExceeded { get; set; }
    }
}

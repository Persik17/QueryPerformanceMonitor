using System;
using System.Collections.Generic;

namespace QueryPerformanceMonitor.Core
{
    public class QueryExecutionInfo
    {
        public string Query { get; set; }
        public TimeSpan Duration { get; set; }
        public string Source { get; set; }
        public string DatabaseProvider { get; set; }
        public DateTime Timestamp { get; set; }
        public string ConnectionString { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public string StackTrace { get; set; }
        public bool IsSlowQuery { get; set; }
    }
}

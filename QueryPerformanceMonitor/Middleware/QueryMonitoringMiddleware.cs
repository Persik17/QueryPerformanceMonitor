using QueryPerformanceMonitor.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace QueryPerformanceMonitor.Middleware
{
    public class QueryMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IQueryPerformanceMonitor _monitor;
        private readonly ILogger<QueryMonitoringMiddleware> _logger;

        public QueryMonitoringMiddleware(
            RequestDelegate next,
            IQueryPerformanceMonitor monitor,
            ILogger<QueryMonitoringMiddleware> logger = null)
        {
            _next = next;
            _monitor = monitor;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestQueries = new List<QueryExecutionInfo>();
            var slowQueries = new List<SlowQueryInfo>();

            Action<QueryExecutionInfo> queryHandler = query => requestQueries.Add(query);
            Action<SlowQueryInfo> slowQueryHandler = slowQuery => slowQueries.Add(slowQuery);

            _monitor.QueryExecuted += queryHandler;
            _monitor.SlowQueryDetected += slowQueryHandler;

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                _monitor.QueryExecuted -= queryHandler;
                _monitor.SlowQueryDetected -= slowQueryHandler;

                // Добавляем заголовки с информацией о производительности
                AddPerformanceHeaders(context, requestQueries, slowQueries, stopwatch.Elapsed);

                // Логируем информацию о запросе
                LogRequestPerformance(context, requestQueries, slowQueries, stopwatch.Elapsed);
            }
        }

        private void AddPerformanceHeaders(
            HttpContext context,
            List<QueryExecutionInfo> queries,
            List<SlowQueryInfo> slowQueries,
            TimeSpan totalDuration)
        {
            if (context.Response.HasStarted)
            {
                _logger?.LogWarning("Response has already started, cannot add performance headers.");
                return;
            }

            if (!context.Response.Headers.ContainsKey("X-Total-Queries"))
            {
                context.Response.Headers.Add("X-Total-Queries", queries.Count.ToString());
            }
            if (!context.Response.Headers.ContainsKey("X-Slow-Queries"))
            {
                context.Response.Headers.Add("X-Slow-Queries", slowQueries.Count.ToString());
            }
            if (!context.Response.Headers.ContainsKey("X-Total-Duration"))
            {
                context.Response.Headers.Add("X-Total-Duration", totalDuration.TotalMilliseconds.ToString("F2"));
            }

            if (queries.Any())
            {
                var totalQueryTime = queries.Sum(q => q.Duration.TotalMilliseconds);
                if (!context.Response.Headers.ContainsKey("X-Total-Query-Time"))
                {
                    context.Response.Headers.Add("X-Total-Query-Time", totalQueryTime.ToString("F2"));
                }
                if (!context.Response.Headers.ContainsKey("X-Avg-Query-Time"))
                {
                    context.Response.Headers.Add("X-Avg-Query-Time", (totalQueryTime / queries.Count).ToString("F2"));
                }
            }

            if (slowQueries.Any())
            {
                var slowestQuery = slowQueries.OrderByDescending(q => q.Duration).First();
                if (!context.Response.Headers.ContainsKey("X-Slowest-Query"))
                {
                    context.Response.Headers.Add("X-Slowest-Query", slowestQuery.Duration.TotalMilliseconds.ToString("F2"));
                }
            }
        }

        private void LogRequestPerformance(
            HttpContext context,
            List<QueryExecutionInfo> queries,
            List<SlowQueryInfo> slowQueries,
            TimeSpan totalDuration)
        {
            if (slowQueries.Any() || queries.Count > 10) // Логируем если есть медленные запросы или много запросов
            {
                _logger?.LogWarning(
                    "Request {Method} {Path} executed {QueryCount} queries ({SlowCount} slow) in {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    queries.Count,
                    slowQueries.Count,
                    totalDuration.TotalMilliseconds);
            }
        }
    }

    public static class QueryMonitoringMiddlewareExtensions
    {
        public static IApplicationBuilder UseQueryPerformanceMonitoring(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<QueryMonitoringMiddleware>();
        }
    }
}

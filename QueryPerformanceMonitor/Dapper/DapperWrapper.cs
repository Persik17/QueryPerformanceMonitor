using Dapper;
using QueryPerformanceMonitor.Core;
using Microsoft.Extensions.Options;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace QueryPerformanceMonitor.Dapper
{
    public class DapperWrapper
    {
        private readonly IDbConnection _connection;
        private readonly IQueryPerformanceMonitor _monitor;
        private readonly QueryMonitoringOptions _options;

        public DapperWrapper(
            IDbConnection connection,
            IQueryPerformanceMonitor monitor,
            IOptions<QueryMonitoringOptions> options)
        {
            _connection = connection;
            _monitor = monitor;
            _options = options.Value;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null)
        {
            return await ExecuteWithMonitoring(
                async () => await _connection.QueryAsync<T>(sql, param, transaction),
                sql, param);
        }

        public async Task<T> QuerySingleAsync<T>(string sql, object param = null, IDbTransaction transaction = null)
        {
            return await ExecuteWithMonitoring(
                async () => await _connection.QuerySingleAsync<T>(sql, param, transaction),
                sql, param);
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, IDbTransaction transaction = null)
        {
            return await ExecuteWithMonitoring(
                async () => await _connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction),
                sql, param);
        }

        public async Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null)
        {
            return await ExecuteWithMonitoring(
                async () => await _connection.ExecuteAsync(sql, param, transaction),
                sql, param);
        }

        private async Task<T> ExecuteWithMonitoring<T>(Func<Task<T>> operation, string sql, object param = null)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                return await operation();
            }
            finally
            {
                stopwatch.Stop();
                RegisterQuery(sql, stopwatch.Elapsed, param);
            }
        }

        private void RegisterQuery(string sql, TimeSpan duration, object param = null)
        {
            var queryInfo = new QueryExecutionInfo
            {
                Query = TruncateQuery(sql),
                Duration = duration,
                Source = "Dapper",
                DatabaseProvider = GetDatabaseProvider(),
                ConnectionString = MaskConnectionString(_connection.ConnectionString)
            };

            if (_options.IncludeParameters && param != null)
            {
                var properties = param.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    queryInfo.Parameters[prop.Name] = prop.GetValue(param);
                }
            }

            _monitor.RegisterQuery(queryInfo);
        }

        private string GetDatabaseProvider()
        {
            return _connection.GetType().Name switch
            {
                "SqlConnection" => "SqlServer",
                "NpgsqlConnection" => "PostgreSQL",
                _ => _connection.GetType().Name
            };
        }

        private string TruncateQuery(string query)
        {
            if (string.IsNullOrEmpty(query) || query.Length <= _options.MaxQueryLength)
                return query;

            return query.Substring(0, _options.MaxQueryLength) + "...";
        }

        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return connectionString;

            return Regex.Replace(connectionString,
                @"(password|pwd)\s*=\s*[^;]+",
                "$1=***",
                RegexOptions.IgnoreCase);
        }
    }
}

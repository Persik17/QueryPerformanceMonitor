using QueryPerformanceMonitor.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace QueryPerformanceMonitor.EntityFramework
{
    public class EfQueryInterceptor : DbCommandInterceptor
    {
        private readonly IQueryPerformanceMonitor _monitor;
        private readonly QueryMonitoringOptions _options;

        public EfQueryInterceptor(
            IQueryPerformanceMonitor monitor,
            IOptions<QueryMonitoringOptions> options)
        {
            _monitor = monitor;
            _options = options.Value;
        }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            RegisterQuery(command, eventData);
            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            RegisterQuery(command, eventData);
            return base.ReaderExecuted(command, eventData, result);
        }

        public override async ValueTask<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            RegisterQuery(command, eventData);
            return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override int NonQueryExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result)
        {
            RegisterQuery(command, eventData);
            return base.NonQueryExecuted(command, eventData, result);
        }

        private void RegisterQuery(DbCommand command, CommandExecutedEventData eventData)
        {
            var queryInfo = new QueryExecutionInfo
            {
                Query = TruncateQuery(command.CommandText),
                Duration = eventData.Duration,
                Source = "Entity Framework",
                DatabaseProvider = GetDatabaseProvider(eventData.Context),
                ConnectionString = MaskConnectionString(command.Connection?.ConnectionString)
            };

            if (_options.IncludeParameters && command.Parameters.Count > 0)
            {
                foreach (DbParameter parameter in command.Parameters)
                {
                    queryInfo.Parameters[parameter.ParameterName] = parameter.Value;
                }
            }

            _monitor.RegisterQuery(queryInfo);
        }

        private string GetDatabaseProvider(DbContext context)
        {
            var providerName = context?.Database.ProviderName;
            return providerName switch
            {
                "Microsoft.EntityFrameworkCore.SqlServer" => "SqlServer",
                "Npgsql.EntityFrameworkCore.PostgreSQL" => "PostgreSQL",
                _ => providerName ?? "Unknown"
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

            // Простое маскирование паролей
            return Regex.Replace(connectionString,
                @"(password|pwd)\s*=\s*[^;]+",
                "$1=***",
                RegexOptions.IgnoreCase);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using System.Data;
using Npgsql;
using QueryPerformanceMonitor.Core;
using QueryPerformanceMonitor.EntityFramework;
using QueryPerformanceMonitor.Dapper;

namespace QueryPerformanceMonitor.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQueryPerformanceMonitoring(
            this IServiceCollection services,
            Action<QueryMonitoringOptions> configure = null)
        {
            var options = new QueryMonitoringOptions();
            configure?.Invoke(options);

            services.Configure<QueryMonitoringOptions>(opt =>
            {
                opt.DefaultSlowQueryThreshold = options.DefaultSlowQueryThreshold;
                opt.SqlServerSlowQueryThreshold = options.SqlServerSlowQueryThreshold;
                opt.PostgreSqlSlowQueryThreshold = options.PostgreSqlSlowQueryThreshold;
                opt.IncludeStackTrace = options.IncludeStackTrace;
                opt.IncludeParameters = options.IncludeParameters;
                opt.MaxQueryLength = options.MaxQueryLength;
            });

            services.AddSingleton<IQueryPerformanceMonitor>(provider =>
            {
                var optionsAccessor = provider.GetRequiredService<IOptions<QueryMonitoringOptions>>();
                var logger = provider.GetService<ILogger<QueryPerformanceMonitorService>>();
                var monitor = new QueryPerformanceMonitorService(optionsAccessor, logger);

                // Подписываем обработчики
                foreach (var handler in options.SlowQueryHandlers)
                {
                    monitor.SlowQueryDetected += handler;
                }

                foreach (var handler in options.QueryHandlers)
                {
                    monitor.QueryExecuted += handler;
                }

                return monitor;
            });

            services.AddScoped<EfQueryInterceptor>();

            return services;
        }

        public static IServiceCollection AddDapperWrapper<TConnection>(
            this IServiceCollection services,
            string connectionString)
            where TConnection : class, IDbConnection, new()
        {
            services.AddScoped<IDbConnection>(provider =>
            {
                var connection = new TConnection
                {
                    ConnectionString = connectionString
                };
                return connection;
            });

            services.AddScoped<DapperWrapper>();

            return services;
        }

        public static IServiceCollection AddDapperWrapperForSqlServer(
            this IServiceCollection services,
            string connectionString)
        {
            return services.AddDapperWrapper<SqlConnection>(connectionString);
        }

        public static IServiceCollection AddDapperWrapperForPostgreSQL(
            this IServiceCollection services,
            string connectionString)
        {
            return services.AddDapperWrapper<NpgsqlConnection>(connectionString);
        }
    }
}

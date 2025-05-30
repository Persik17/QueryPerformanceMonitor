using System.Data;

namespace QueryPerformanceMonitor.Providers
{
    public class PostgreSqlQueryProvider : IQueryProvider
    {
        public string ProviderName => "PostgreSQL";

        public string OptimizeQuery(string query)
        {
            // Специфичные для PostgreSQL оптимизации
            return query;
        }

        public Dictionary<string, object> GetPerformanceMetrics(IDbConnection connection)
        {
            var metrics = new Dictionary<string, object>();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        numbackends,
                        xact_commit,
                        xact_rollback,
                        blks_read,
                        blks_hit
                    FROM pg_stat_database 
                    WHERE datname = current_database()";

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    metrics["ActiveConnections"] = reader["numbackends"];
                    metrics["CommittedTransactions"] = reader["xact_commit"];
                    metrics["RolledBackTransactions"] = reader["xact_rollback"];
                    metrics["BlocksRead"] = reader["blks_read"];
                    metrics["BlocksHit"] = reader["blks_hit"];
                }
            }
            catch (Exception ex)
            {
                metrics["Error"] = ex.Message;
            }

            return metrics;
        }
    }
}

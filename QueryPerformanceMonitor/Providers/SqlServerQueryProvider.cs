using System;
using System.Collections.Generic;
using System.Data;

namespace QueryPerformanceMonitor.Providers
{
    public class SqlServerQueryProvider : IQueryProvider
    {
        public string ProviderName => "SqlServer";

        public string OptimizeQuery(string query)
        {
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
                        @@CONNECTIONS as TotalConnections,
                        @@CPU_BUSY as CpuBusy,
                        @@IO_BUSY as IoBusy";

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    metrics["TotalConnections"] = reader["TotalConnections"];
                    metrics["CpuBusy"] = reader["CpuBusy"];
                    metrics["IoBusy"] = reader["IoBusy"];
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

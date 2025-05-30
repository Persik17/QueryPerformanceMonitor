using QueryPerformanceMonitor.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace QueryPerformanceMonitor.Extensions
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder AddQueryPerformanceMonitoring(
            this DbContextOptionsBuilder optionsBuilder,
            IServiceProvider serviceProvider)
        {
            var interceptor = serviceProvider.GetRequiredService<EfQueryInterceptor>();
            return optionsBuilder.AddInterceptors(interceptor);
        }

        public static DbContextOptionsBuilder<T> AddQueryPerformanceMonitoring<T>(
            this DbContextOptionsBuilder<T> optionsBuilder,
            IServiceProvider serviceProvider) where T : DbContext
        {
            var interceptor = serviceProvider.GetRequiredService<EfQueryInterceptor>();
            return optionsBuilder.AddInterceptors(interceptor);
        }
    }
}

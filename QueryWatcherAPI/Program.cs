using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using QueryPerformanceMonitor.Extensions;
using QueryPerformanceMonitorAPI.Data;
using QueryPerformanceMonitor.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Добавляем мониторинг производительности
builder.Services.AddQueryPerformanceMonitoring(options =>
{
    options.SqlServerSlowQueryThreshold = TimeSpan.FromMilliseconds(1);
    options.PostgreSqlSlowQueryThreshold = TimeSpan.FromMilliseconds(1);
    options.IncludeStackTrace = builder.Environment.IsDevelopment();
    options.IncludeParameters = builder.Environment.IsDevelopment();
    options.MaxQueryLength = 2000;

    // Добавляем обработчики медленных запросов
    options.SlowQueryHandlers.Add(slowQuery =>
    {
        Console.WriteLine($"   Slow query detected!");
        Console.WriteLine($"   Duration: {slowQuery.Duration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"   Provider: {slowQuery.DatabaseProvider}");
        Console.WriteLine($"   Source: {slowQuery.Source}");
        Console.WriteLine($"   Query: {slowQuery.Query[..Math.Min(100, slowQuery.Query.Length)]}...");
    });
});

// Настройка Entity Framework с мониторингом
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddQueryPerformanceMonitoring(serviceProvider);
});

// Альтернативно для PostgreSQL
builder.Services.AddDbContext<PostgresDbContext>((serviceProvider, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"))
           .AddQueryPerformanceMonitoring(serviceProvider);
});

// Добавляем Dapper wrapper
//builder.Services.AddDapperWrapperForSqlServer(
//    builder.Configuration.GetConnectionString("DefaultConnection"));

// Или для PostgreSQL
builder.Services.AddDapperWrapperForPostgreSQL(
    builder.Configuration.GetConnectionString("PostgresConnection"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1"); // Явное указание пути к swagger.json
    });
}

app.UseRouting();

// Добавляем middleware для мониторинга
app.UseQueryPerformanceMonitoring();

app.MapControllers();

app.Run();
# Монитор Производительности Запросов

Этот пакет предоставляет простой способ мониторинга производительности запросов в ваших ASP.NET Core приложениях, использующих Entity Framework Core (EF Core) и Dapper. Он поддерживает мониторинг запросов для баз данных MSSQL и PostgreSQL.

## Возможности

*   Автоматическое обнаружение медленных запросов на основе заданного порога.
*   Поддержка EF Core и Dapper.
*   Поддержка MSSQL и PostgreSQL.
*   Включение стека вызовов и параметров запроса (опционально).
*   Настраиваемые обработчики для медленных запросов и всех запросов.
*   Middleware для добавления заголовков производительности в ответы HTTP.

## Установка

1.  Установите пакет NuGet:

    ```powershell
    Install-Package Your.QueryPerformanceMonitor.Package.Name
    ```

    Замените `Your.QueryPerformanceMonitor.Package.Name` на имя вашего пакета NuGet.

## Использование

### 1. Настройка служб

В файле `Program.cs` (или `Startup.cs` в старых версиях ASP.NET Core) добавьте следующие строки:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Добавляем мониторинг производительности
builder.Services.AddQueryPerformanceMonitoring(options =>
{
    options.SqlServerSlowQueryThreshold = TimeSpan.FromMilliseconds(1); // Порог медленного запроса для SQL Server (1 мс)
    options.PostgreSqlSlowQueryThreshold = TimeSpan.FromMilliseconds(1); // Порог медленного запроса для PostgreSQL (1 мс)
    options.IncludeStackTrace = builder.Environment.IsDevelopment();   // Включать стек вызовов только в режиме разработки
    options.IncludeParameters = builder.Environment.IsDevelopment();   // Включать параметры запроса только в режиме разработки
    options.MaxQueryLength = 2000;                                      // Максимальная длина запроса для логирования

    // Добавляем обработчик медленных запросов
    options.SlowQueryHandlers.Add(slowQuery =>
    {
        Console.WriteLine($"🐌 Slow query detected!");
        Console.WriteLine($"   Длительность: {slowQuery.Duration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"   Провайдер: {slowQuery.DatabaseProvider}");
        Console.WriteLine($"   Источник: {slowQuery.Source}");
        Console.WriteLine($"   Запрос: {slowQuery.Query[..Math.Min(100, slowQuery.Query.Length)]}...");
    });

    // Обработчик для всех запросов (например, для метрик)
    //options.QueryHandlers.Add(query =>
    //{
    //    // Отправка метрик в систему мониторинга
    //    // MetricsCollector.RecordQueryDuration(query.DatabaseProvider, query.Duration);
    //});
});
```

*   `AddQueryPerformanceMonitoring`: Добавляет сервисы мониторинга производительности в контейнер зависимостей.
*   `SqlServerSlowQueryThreshold` и `PostgreSqlSlowQueryThreshold`: Определяют порог времени выполнения запроса (в миллисекундах), после которого запрос считается медленным. Можно установить разные значения для MSSQL и PostgreSQL.
*   `IncludeStackTrace`: Включает включение стека вызовов в информацию о медленных запросах. Рекомендуется включать только в режиме разработки, так как это может повлиять на производительность.
*   `IncludeParameters`: Включает включение параметров запроса в информацию о запросах. Рекомендуется включать только в режиме разработки.
*   `MaxQueryLength`: Определяет максимальную длину запроса, который будет логироваться. Это позволяет избежать логирования очень больших запросов, которые могут заполнить логи.
*   `SlowQueryHandlers`: Список обработчиков, которые будут вызываться для каждого медленного запроса. Вы можете добавить свои собственные обработчики, чтобы выполнять различные действия, такие как логирование, отправка уведомлений или запись метрик.
*   `QueryHandlers`: Список обработчиков, которые будут вызываться для каждого запроса. Может использоваться для сбора общей статистики, агрегации или отправки в системы мониторинга.

### 2. Настройка DbContext (Entity Framework Core)

Для мониторинга запросов EF Core, необходимо добавить метод расширения `AddQueryPerformanceMonitoring` к вашему `DbContextOptionsBuilder`:

```csharp
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
```

### 3. Настройка Dapper (опционально)

Для мониторинга запросов Dapper, необходимо зарегистрировать `DapperWrapper`.

```csharp
// Добавляем Dapper wrapper
//builder.Services.AddDapperWrapperForSqlServer(
//    builder.Configuration.GetConnectionString("DefaultConnection"));

// Или для PostgreSQL
builder.Services.AddDapperWrapperForPostgreSQL(
    builder.Configuration.GetConnectionString("PostgresConnection"));
```

### 4. Добавление Middleware

Добавьте middleware `UseQueryPerformanceMonitoring` в конвейер обработки запроса перед `UseRouting()`, `UseEndpoints()` и `UseSwagger()` (если используете Swagger):

```csharp
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
```

### 5. Использование

Теперь ваш пакет для мониторинга производительности готов к работе. Он автоматически будет перехватывать запросы EF Core и Dapper, измерять время их выполнения и вызывать настроенные обработчики.

Медленные запросы будут регистрироваться в консоли (или в другом месте, в зависимости от ваших настроек).

## Настройка

Вы можете настроить параметры мониторинга, передав делегат в метод `AddQueryPerformanceMonitoring`.

```csharp
builder.Services.AddQueryPerformanceMonitoring(options =>
{
    // Настройки...
});
```

Доступные параметры:

*   `SqlServerSlowQueryThreshold`: Порог медленного запроса для SQL Server (TimeSpan).
*   `PostgreSqlSlowQueryThreshold`: Порог медленного запроса для PostgreSQL (TimeSpan).
*   `IncludeStackTrace`: Включать стек вызовов в информацию о медленных запросах (bool).
*   `IncludeParameters`: Включать параметры запроса в информацию о запросах (bool).
*   `MaxQueryLength`: Максимальная длина запроса для логирования (int).
*   `SlowQueryHandlers`: Список обработчиков медленных запросов (List<Action<SlowQueryInfo>>).
*   `QueryHandlers`: Список обработчиков всех запросов (List<Action<QueryExecutionInfo>>).

## Поддержка баз данных

Этот пакет поддерживает мониторинг производительности для следующих баз данных:

*   Microsoft SQL Server (MSSQL)
*   PostgreSQL

## Дополнительно

Вы можете добавлять свои собственные обработчики для медленных запросов и всех запросов, чтобы выполнять различные действия, такие как логирование, отправка уведомлений или запись метрик.

## Заключение

Этот пакет предоставляет простой и эффективный способ мониторинга производительности запросов в ваших ASP.NET Core приложениях. Он поможет вам выявить и устранить медленные запросы, что приведет к улучшению производительности вашего приложения.
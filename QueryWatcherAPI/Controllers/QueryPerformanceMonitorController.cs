using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueryPerformanceMonitor.Dapper;
using QueryPerformanceMonitorAPI.Data;
using QueryPerformanceMonitorAPI.Data.DTOs;
using QueryPerformanceMonitorAPI.Data.Entities;

namespace QueryWatcherAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PostgresDbContext _pgsqlContext;
        private readonly DapperWrapper _dapper;

        public UsersController(ApplicationDbContext context, PostgresDbContext pgsqlContext, DapperWrapper dapper)
        {
            _context = context;
            _pgsqlContext = pgsqlContext;
            _dapper = dapper;
        }

        [HttpGet("ef_mssql")]
        public async Task<IActionResult> GetUsersWithEfFromMSSQL()
        {
            // Ётот запрос будет автоматически отслеживатьс€
            var users = await _context.Users
                .Include(u => u.Orders)
                .Where(u => u.IsActive)
                .ToListAsync();

            var userDtos = users.Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                IsActive = u.IsActive,
                Orders = [.. u.Orders.Select(o => new OrderResponse
                {
                    Id = o.Id,
                    Total = o.Total,
                    Status = o.Status
                })]
            }).ToList();

            return Ok(userDtos);
        }

        [HttpGet("ef_postgresql")]
        public async Task<IActionResult> GetUsersWithEfFromPostgreSQL()
        {
            // Ётот запрос будет автоматически отслеживатьс€
            var users = await _pgsqlContext.Users
                .Include(u => u.Orders)
                .Where(u => u.IsActive)
                .ToListAsync();

            var userDtos = users.Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                IsActive = u.IsActive,
                Orders = [.. u.Orders.Select(o => new OrderResponse
                {
                    Id = o.Id,
                    Total = o.Total,
                    Status = o.Status
                })]
            }).ToList();

            return Ok(userDtos);
        }

        [HttpGet("dapper")]
        public async Task<IActionResult> GetUsersWithDapperFromMSSQL()
        {
            // Ётот запрос также будет отслеживатьс€ через DapperWrapper
            var users = await _dapper.QueryAsync<User>(@"
            SELECT u.*, o.Id as OrderId, o.Total 
            FROM Users u 
            LEFT JOIN Orders o ON u.Id = o.UserId 
            WHERE u.IsActive = @IsActive",
                new { IsActive = true });

            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserToMSSQL([FromBody] CreateUserRequest request)
        {
            //  омбинированное использование EF и Dapper
            var user = new User { Name = request.Name, Email = request.Email };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // EF запрос

            // ƒополнительна€ логика через Dapper
            await _dapper.ExecuteAsync(
                "INSERT INTO UserAudit (UserId, Action, Timestamp) VALUES (@UserId, @Action, @Timestamp)",
                new { UserId = user.Id, Action = "Created", Timestamp = DateTime.UtcNow });

            return Ok(user);
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateUserToPostgreSQL([FromBody] CreateUserRequest request)
        //{
        //    //  омбинированное использование EF и Dapper
        //    var user = new User { Name = request.Name, Email = request.Email };

        //    _pgsqlContext.Users.Add(user);
        //    await _pgsqlContext.SaveChangesAsync(); // EF запрос

        //    // ƒополнительна€ логика через Dapper
        //    await _dapper.ExecuteAsync(
        //        "INSERT INTO UserAudit (UserId, Action, Timestamp) VALUES (@UserId, @Action, @Timestamp)",
        //        new { UserId = user.Id, Action = "Created", Timestamp = DateTime.UtcNow });

        //    return Ok(user);
        //}
    }
}
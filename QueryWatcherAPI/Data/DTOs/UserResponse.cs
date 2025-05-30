namespace QueryPerformanceMonitorAPI.Data.DTOs
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderResponse> Orders { get; set; } = new();
    }
}

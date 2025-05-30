namespace QueryPerformanceMonitorAPI.Data.DTOs
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
    }
}

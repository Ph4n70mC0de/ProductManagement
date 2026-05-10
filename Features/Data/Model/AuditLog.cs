namespace ProductManagement.Features.Data.Model
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
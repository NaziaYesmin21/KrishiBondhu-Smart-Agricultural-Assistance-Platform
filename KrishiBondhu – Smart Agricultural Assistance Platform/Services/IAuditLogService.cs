public interface IAuditLogService
{
    Task LogAsync(
        string username,
        string action,
        string? description = null);

    Task<List<AuditLog>> GetAllLogsAsync();
}
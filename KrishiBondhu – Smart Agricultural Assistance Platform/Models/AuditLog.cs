using System;

public class AuditLog
{
    public int AuditLogId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
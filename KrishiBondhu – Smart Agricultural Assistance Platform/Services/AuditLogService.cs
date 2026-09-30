using Microsoft.EntityFrameworkCore;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;

    public AuditLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(
        string username,
        string action,
        string? description = null)
    {
        var log = new AuditLog
        {
            Username = username,
            Action = action,
            Description = description,

            // PostgreSQL timestamp without time zone
            Timestamp = DateTime.Now
        };

        _context.AuditLogs.Add(log);

        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetAllLogsAsync()
    {
        return await _context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }
}
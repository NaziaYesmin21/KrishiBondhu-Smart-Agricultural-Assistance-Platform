using Microsoft.EntityFrameworkCore;

public class HarvestReminderService
{
    private readonly ApplicationDbContext _context;

    public HarvestReminderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Cultivation>> GetUpcomingHarvestsAsync()
    {
        var today = DateTime.Now.Date;
        var reminderDate = today.AddDays(7);

        return await _context.Cultivations
            .Where(c =>
                c.HarvestDate.HasValue &&
                c.HarvestDate.Value.Date >= today &&
                c.HarvestDate.Value.Date <= reminderDate)
            .OrderBy(c => c.HarvestDate)
            .ToListAsync();
    }
}
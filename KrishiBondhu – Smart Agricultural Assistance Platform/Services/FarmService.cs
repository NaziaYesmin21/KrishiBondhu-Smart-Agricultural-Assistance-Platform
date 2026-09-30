using Microsoft.EntityFrameworkCore;

public class FarmService : IFarmService
{
    private readonly ApplicationDbContext _context;

    public FarmService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Farm>> GetAllFarmsAsync()
    {
        return await _context.Farms
            .Include(f => f.Farmer)
            .ToListAsync();
    }

    public async Task<Farm?> GetFarmByIdAsync(int id)
    {
        return await _context.Farms
            .Include(f => f.Farmer)
            .FirstOrDefaultAsync(f => f.FarmId == id);
    }

    public async Task CreateFarmAsync(Farm farm)
    {
        _context.Farms.Add(farm);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateFarmAsync(Farm farm)
    {
        var existingFarm = await _context.Farms
            .FindAsync(farm.FarmId);

        if (existingFarm == null)
        {
            return false;
        }

        _context.Entry(existingFarm)
            .CurrentValues
            .SetValues(farm);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteFarmAsync(int id)
    {
        var farm = await _context.Farms
            .FindAsync(id);

        if (farm == null)
        {
            return false;
        }

        _context.Farms.Remove(farm);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> FarmExistsAsync(int id)
    {
        return await _context.Farms
            .AnyAsync(f => f.FarmId == id);
    }
}
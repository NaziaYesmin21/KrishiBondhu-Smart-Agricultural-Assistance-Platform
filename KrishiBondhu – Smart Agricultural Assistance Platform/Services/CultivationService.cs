using Microsoft.EntityFrameworkCore;

public class CultivationService : ICultivationService
{
    private readonly ApplicationDbContext _context;

    public CultivationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Cultivation>> GetAllCultivationsAsync()
    {
        return await _context.Cultivations
            .Include(c => c.Farm)
            .Include(c => c.Crop)
            .ToListAsync();
    }

    public async Task<Cultivation?> GetCultivationByIdAsync(int id)
    {
        return await _context.Cultivations
            .Include(c => c.Farm)
            .Include(c => c.Crop)
            .FirstOrDefaultAsync(c => c.CultivationId == id);
    }

    public async Task CreateCultivationAsync(Cultivation cultivation)
    {
        _context.Cultivations.Add(cultivation);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateCultivationAsync(Cultivation cultivation)
    {
        var existingCultivation = await _context.Cultivations
            .FindAsync(cultivation.CultivationId);

        if (existingCultivation == null)
        {
            return false;
        }

        existingCultivation.FarmId = cultivation.FarmId;
        existingCultivation.CropId = cultivation.CropId;
        existingCultivation.PlantingDate = cultivation.PlantingDate;
        existingCultivation.HarvestDate = cultivation.HarvestDate;
        existingCultivation.Yield = cultivation.Yield;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteCultivationAsync(int id)
    {
        var cultivation = await _context.Cultivations
            .FindAsync(id);

        if (cultivation == null)
        {
            return false;
        }

        _context.Cultivations.Remove(cultivation);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CultivationExistsAsync(int id)
    {
        return await _context.Cultivations
            .AnyAsync(c => c.CultivationId == id);
    }
}
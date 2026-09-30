using Microsoft.EntityFrameworkCore;

public class CropService : ICropService
{
    private readonly ApplicationDbContext _context;

    public CropService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Crop>> GetAllCropsAsync()
    {
        return await _context.Crops.ToListAsync();
    }

    public async Task<Crop?> GetCropByIdAsync(int id)
    {
        return await _context.Crops
            .FirstOrDefaultAsync(c => c.CropId == id);
    }

    public async Task CreateCropAsync(Crop crop)
    {
        _context.Crops.Add(crop);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateCropAsync(Crop crop)
    {
        var existingCrop = await _context.Crops
            .FindAsync(crop.CropId);

        if (existingCrop == null)
        {
            return false;
        }

        _context.Entry(existingCrop).CurrentValues.SetValues(crop);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteCropAsync(int id)
    {
        var crop = await _context.Crops.FindAsync(id);

        if (crop == null)
        {
            return false;
        }

        _context.Crops.Remove(crop);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CropExistsAsync(int id)
    {
        return await _context.Crops
            .AnyAsync(c => c.CropId == id);
    }
}
using Microsoft.EntityFrameworkCore;

public class DiseaseService : IDiseaseService
{
    private readonly ApplicationDbContext _context;

    public DiseaseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Disease>> GetAllDiseasesAsync()
    {
        return await _context.Diseases
            .Include(d => d.Crop)
            .ToListAsync();
    }

    public async Task<Disease?> GetDiseaseByIdAsync(int id)
    {
        return await _context.Diseases
            .Include(d => d.Crop)
            .FirstOrDefaultAsync(d => d.DiseaseId == id);
    }

    public async Task CreateDiseaseAsync(Disease disease)
    {
        _context.Diseases.Add(disease);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateDiseaseAsync(Disease disease)
    {
        var existingDisease = await _context.Diseases
            .FindAsync(disease.DiseaseId);

        if (existingDisease == null)
        {
            return false;
        }

        _context.Entry(existingDisease)
            .CurrentValues
            .SetValues(disease);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteDiseaseAsync(int id)
    {
        var disease = await _context.Diseases
            .FindAsync(id);

        if (disease == null)
        {
            return false;
        }

        _context.Diseases.Remove(disease);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DiseaseExistsAsync(int id)
    {
        return await _context.Diseases
            .AnyAsync(d => d.DiseaseId == id);
    }
}
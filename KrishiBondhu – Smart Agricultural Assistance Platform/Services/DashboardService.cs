using Microsoft.EntityFrameworkCore;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetFarmerCountAsync()
    {
        return await _context.Farmers.CountAsync();
    }

    public async Task<int> GetFarmCountAsync()
    {
        return await _context.Farms.CountAsync();
    }

    public async Task<int> GetCropCountAsync()
    {
        return await _context.Crops.CountAsync();
    }

    public async Task<int> GetCultivationCountAsync()
    {
        return await _context.Cultivations.CountAsync();
    }

    public async Task<int> GetSoilTestCountAsync()
    {
        return await _context.SoilTests.CountAsync();
    }

    public async Task<int> GetDiseaseCountAsync()
    {
        return await _context.Diseases.CountAsync();
    }

    public async Task<List<CropStatistics>> GetCropStatisticsAsync()
    {
        return await _context.Cultivations
            .Include(c => c.Crop)
            .GroupBy(c => c.Crop.CropName)
            .Select(g => new CropStatistics
            {
                CropName = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
    }

    public async Task<List<DiseaseStatistics>> GetDiseaseStatisticsAsync()
    {
        return await _context.Diseases
            .Include(d => d.Crop)
            .GroupBy(d => d.DiseaseName)
            .Select(g => new DiseaseStatistics
            {
                DiseaseName = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
    }
}
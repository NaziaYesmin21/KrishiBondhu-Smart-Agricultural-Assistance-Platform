using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDashboardService
{
    Task<int> GetFarmerCountAsync();

    Task<int> GetFarmCountAsync();

    Task<int> GetCropCountAsync();

    Task<int> GetCultivationCountAsync();

    Task<int> GetSoilTestCountAsync();

    Task<int> GetDiseaseCountAsync();

    Task<List<CropStatistics>> GetCropStatisticsAsync();

    Task<List<DiseaseStatistics>> GetDiseaseStatisticsAsync();
}
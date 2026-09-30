using System.Threading.Tasks;

public interface IDashboardService
{
    Task<int> GetFarmerCountAsync();
    Task<int> GetFarmCountAsync();
    Task<int> GetCropCountAsync();
    Task<int> GetCultivationCountAsync();
    Task<int> GetSoilTestCountAsync();
    Task<int> GetDiseaseCountAsync();
}
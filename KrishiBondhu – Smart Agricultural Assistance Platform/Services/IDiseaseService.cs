using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDiseaseService
{
    Task<List<Disease>> GetAllDiseasesAsync();

    Task<Disease?> GetDiseaseByIdAsync(int id);

    Task CreateDiseaseAsync(Disease disease);

    Task<bool> UpdateDiseaseAsync(Disease disease);

    Task<bool> DeleteDiseaseAsync(int id);

    Task<bool> DiseaseExistsAsync(int id);
}
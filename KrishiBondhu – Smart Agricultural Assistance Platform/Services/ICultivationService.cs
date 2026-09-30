using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICultivationService
{
    Task<List<Cultivation>> GetAllCultivationsAsync();

    Task<Cultivation?> GetCultivationByIdAsync(int id);

    Task CreateCultivationAsync(Cultivation cultivation);

    Task<bool> UpdateCultivationAsync(Cultivation cultivation);

    Task<bool> DeleteCultivationAsync(int id);

    Task<bool> CultivationExistsAsync(int id);
}
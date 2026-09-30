using System.Threading.Tasks;

public interface IReportService
{
    Task<byte[]> GenerateFarmerReportAsync(int farmerId);
}
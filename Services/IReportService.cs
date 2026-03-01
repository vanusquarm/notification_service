using System.Threading.Tasks;

namespace GTBStatementService.Services
{
    public interface IReportService
    {
        Task<byte[]> GetStatementReportAsync(string customerNo, string format, string accountList);
    }
}

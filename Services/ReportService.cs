using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GTBStatementService.Services
{
    public class ReportService : IReportService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ILogger<ReportService> _logger;
        private readonly IStatementRepository _repository;

        public ReportService(HttpClient httpClient, IConfiguration configuration, ILogger<ReportService> logger, IStatementRepository repository)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["StatementSettings:CoreBaseUrl"] ?? string.Empty;
            _logger = logger;
            _repository = repository;
        }

        public async Task<byte[]> GetStatementReportAsync(string customerNo, string format, string accountList)
        {
            // Example endpoint: http://core-api/v1/api/reports/ExportFile/ReportFile?cusNo=123&accounts=123,456,789&format=pdf
            string requestUrl = $"{_baseUrl}ExportFile/ReportFile?customerNo={customerNo}&format={format}&accounts={Uri.EscapeDataString(accountList)}";

            try
            {
                _logger.LogInformation("Fetching report from: {Url}", requestUrl);
                var response = await _httpClient.GetAsync(requestUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                var errorMsg = await response.Content.ReadAsStringAsync();
                _logger.LogError("API Error fetching report for {CustomerNo}: {Error}", customerNo, errorMsg);
                throw new Exception($"Failed to fetch report from API. Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while fetching report for {CustomerNo}", customerNo);
                throw;
            }
        }

        public byte[] GetStatementReport(string customerNo, string format, string accountList)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching report for customer {CustomerNo}, format {Format}, accounts {AccountList}",
                    customerNo, format, accountList);

                var from = DateTime.Now.AddDays(-30);
                var to = DateTime.Now;

                var txns = _repository.GetAccountTransactions(accountList, from, to);

                var pdf = new BankStatementPdf(header, txns);

                // Generate PDF into memory
                return pdf.GeneratePdfBytes();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Exception while fetching report for {CustomerNo}",
                    customerNo);
                throw;
            }
        }

        private byte[] GeneratePdfBytes()
        {
            using var stream = new MemoryStream();
            document.Save(stream); 
            return stream.ToArray();
        }
    }
}

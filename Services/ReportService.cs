using System.Net.Http;
using GTBStatementService.Data;

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
            _repository = repository;        }

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

                var txns = _repository.GetAccountStatements(accountList, from, to);
                var statement = new BankStatement
                {
                    BankName = "GUARANTY TRUST BANK (LIBERIA) LIMITED",
                    CustomerName = "MULBAH, SUMO KOLLIE",
                    AccountNumber = "0800824/002/0001/000",
                    AccountType = "CURRENT ACCOUNT",
                    Currency = "USD",
                    PeriodFrom = new DateTime(2023, 2, 1),
                    PeriodTo = new DateTime(2026, 2, 28),
                    Transactions = txns
                };

                var pdf = new PdfService(statement);

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
    }
}

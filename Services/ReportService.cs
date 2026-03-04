using GTBStatementService.Data;
using GTBStatementService.Data.Mock;
using GTBStatementService.Models;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Generic;
using System.Net.Http;

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

        public async Task<List<byte[]>> GetStatementReport(string customerNo, string format, string? accountList = null)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching report for customer {CustomerNo}, format {Format}, accounts {AccountList}",
                    customerNo, format, accountList);

                var from = DateTime.Now.AddDays(-30);
                var to = DateTime.Now;

                var accounts = (!string.IsNullOrEmpty(accountList)) ? accountList?.Split(",")
                    .Select(x => new Account() { AccountId = x, AccountName = "", AccountType = "", Currency = ""})
                    : [];

                if (!accounts.Any())
                    accounts = _repository.GetCustomerAccounts(customerNo).ToList();

                var statements = new List<BankStatement>();

                foreach (var account in accounts)
                {
                    var txns = _repository.GetAccountTransactions(account.AccountId, from, to);

                    statements.Add(new BankStatement
                    {
                        BankName = "GUARANTY TRUST BANK (LIBERIA) LIMITED",
                        AccountName = account.AccountName,
                        AccountNumber = account.AccountId,
                        AccountType = account.AccountType,
                        Currency = account.Currency,
                        PeriodFrom = from,
                        PeriodTo = to,
                        Transactions = txns
                    });
                }

                statements.Add(MockFactory.CreateMock()); // For testing

                // Generate and Save to Disk
                var generator = new StatementPdfGenerator();
                await generator.GenerateAsync(statements, from, to, customerNo);

                // Generate PDF into memory
                return PdfService.GeneratePdfBytes(statements);
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

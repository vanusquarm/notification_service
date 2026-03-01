using GTBStatementService.Data;
using GTBStatementService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GTBStatementService.Services
{
    public class StatementProcessor
    {
        private readonly IStatementRepository _repository;
        private readonly GTMailDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly IReportService _reportService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StatementProcessor> _logger;

        public StatementProcessor(
            IStatementRepository repository,
            GTMailDbContext dbContext,
            IEmailService emailService,
            IReportService reportService,
            IConfiguration configuration,
            ILogger<StatementProcessor> logger)
        {
            _repository = repository;
            _dbContext = dbContext;
            _emailService = emailService;
            _reportService = reportService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task ProcessStatementsAsync()
        {
            _logger.LogInformation("Statement processing started at: {time}", DateTimeOffset.Now);

            // Fetch active profiles (Status 3 = Active)
            var activeProfiles = await _repository.GetCustomerProfileAsync();

            _logger.LogInformation("Found {Count} active profiles to evaluate.", activeProfiles.Count);

            foreach (var profile in activeProfiles)
            {
                try
                {
                    bool shouldSendDaily = IsDailyDue(profile);
                    bool shouldSendWeekly = IsWeeklyDue(profile);
                    bool shouldSendMonthly = IsMonthlyDue(profile);

                    if (shouldSendDaily || shouldSendWeekly || shouldSendMonthly)
                    {
                        string frequencyLabel = shouldSendDaily ? "Daily" : (shouldSendWeekly ? "Weekly" : "Monthly");
                        await ExecuteProcessing(profile, frequencyLabel);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing customer {CustomerNo}", profile.CustomerNo);
                }
            }

            _logger.LogInformation("Statement processing completed at: {time}", DateTimeOffset.Now);
        }

        private bool IsDailyDue(CustomerProfile p)
        {
            if (p.Daily != 1) return false;
            // Send if never sent or last sent was yesterday or earlier
            return !p.LastsentDaily.HasValue || p.LastsentDaily.Value.Date < DateTime.Today;
        }

        private bool IsWeeklyDue(CustomerProfile p)
        {
            if (p.Weekly != 1) return false;
            // Logic: Send if today matches WeeklyOpt day (e.g. "Monday") AND not sent this week
            string currentDay = DateTime.Today.DayOfWeek.ToString();
            bool isTargetDay = string.IsNullOrEmpty(p.WeeklyOpt) || p.WeeklyOpt.Equals(currentDay, StringComparison.OrdinalIgnoreCase);
            
            bool alreadySentThisWeek = p.LastsentWeekly.HasValue && 
                                      (DateTime.Today - p.LastsentWeekly.Value.Date).TotalDays < 7;

            return isTargetDay && !alreadySentThisWeek;
        }

        private bool IsMonthlyDue(CustomerProfile p)
        {
            if (p.Monthly != 1) return false;
            // Logic: Send on the 1st of the month if not sent this month
            // Or use MonthlyOpt as the day of the month
            int targetDay = 1;
            int.TryParse(p.MonthlyOpt, out targetDay);
            if (targetDay == 0) targetDay = 1;

            bool isTargetDay = DateTime.Today.Day == targetDay;
            bool alreadySentThisMonth = p.LastsentMonthly.HasValue && 
                                        p.LastsentMonthly.Value.Month == DateTime.Today.Month &&
                                        p.LastsentMonthly.Value.Year == DateTime.Today.Year;

            return isTargetDay && !alreadySentThisMonth;
        }

        private async Task ExecuteProcessing(CustomerProfile profile, string frequency)
        {
            _logger.LogInformation("Generating {Frequency} statement for {CustomerNo}", frequency, profile.CustomerNo);

            string format = profile.ExportFormat == 1 ? "pdf" : "xls";
            string fileName = $"Statement_{profile.CustomerNo}_{DateTime.Now:yyyyMMdd}.{format}";
            
            // 1. Fetch Report from API
            byte[] fileContent = await _reportService.GetStatementReportAsync(
                profile.CustomerNo, 
                format, 
                profile.Allaccounts ?? string.Empty
            );

            // 2. Send Email
            string subject = $"{_configuration["StatementSettings:EmailSubject"]} - {frequency} ({DateTime.Now:MMM yyyy})";
            string body = $"Dear {profile.DisplayName},<br><br>Please find attached your {frequency} account statement.<br><br>Regards,<br>GTBank";

            await _emailService.SendStatementAsync(profile.Email!, subject, body, fileContent, fileName);

            // 3. Update DB
            if (frequency == "Daily") profile.LastsentDaily = DateTime.Now;
            else if (frequency == "Weekly") profile.LastsentWeekly = DateTime.Now;
            else if (frequency == "Monthly") profile.LastsentMonthly = DateTime.Now;

            _dbContext.Profiles.Update(profile);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Successfully processed and updated record for {CustomerNo}", profile.CustomerNo);
        }
    }
}

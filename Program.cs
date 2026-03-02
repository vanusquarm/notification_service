using GTBStatementService;
using GTBStatementService.Data;
using GTBStatementService.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// Database Configuration
string connectionString = builder.Configuration.GetConnectionString("ConnectGTMail") 
    ?? throw new InvalidOperationException("Connection string 'ConnectGTMail' not found.");

builder.Services.AddDbContext<GTMailDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IStatementRepository, StatementRepository>();

// Register HttpClient for ReportService
builder.Services.AddHttpClient<IReportService, ReportService>();

// Register Application Services
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<StatementProcessor>();

// Register Hosted Service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

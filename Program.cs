using GTBStatementService;
using GTBStatementService.Data;
using GTBStatementService.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Serilog;


try
{
	var builder = Host.CreateApplicationBuilder(args);

	Log.Logger = new LoggerConfiguration()
		.ReadFrom.Configuration(builder.Configuration)
		.CreateLogger();
	builder.Services.AddSerilog();

	QuestPDF.Settings.License = LicenseType.Community;

	// Database Configuration
	string connectionString = builder.Configuration.GetConnectionString("ConnectSQL")
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

	var app = builder.Build();
	app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
	Log.CloseAndFlush();
}
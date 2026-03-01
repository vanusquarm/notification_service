# GTB Statement Service

GTB Statement Service is a .NET 8.0 background worker application designed to automate the delivery of account statements to customers. It periodically checks a SQL Server database for active customer profiles, evaluates their statement frequency requirements (Daily, Weekly, Monthly), fetches the generated reports from a Core API, and dispatches them via SMTP.

## Features

- **Automated Scheduling**: Runs as a background service with configurable intervals.
- **Dynamic Frequency Handling**: Supports Daily, Weekly, and Monthly statement delivery.
- **Reporting Integration**: Fetches PDF or Excel reports from a remote Core API.
- **Reliable Email Delivery**: Uses MailKit/MimeKit for SMTP communication.
- **Database Tracking**: Updates `LastSent` timestamps to prevent duplicate emails.
- **Robust Logging**: Detailed logs for success and failure tracking.

## Prerequisites

- .NET 8.0 SDK
- SQL Server (containing the `GTMail` database and `Profile` table)
- Access to the Core Report API
- Access to the internal SMTP Server (10.230.1.38)

## Installation

1. Clone the repository to your local machine or server.
2. Navigate to the project root directory.
3. Restore dependencies:
    dotnet restore

## Configuration

Update the `appsettings.json` file with your specific environment details:

1. **Connection Strings**: 
   - `ConnectGTMail`: The SQL connection string for the database containing customer profiles.
2. **StatementSettings**:
   - `EmailServer`: The SMTP server IP (default: 10.230.1.38).
   - `PostMasterEmail`: The sender email address.
   - `CoreBaseUrl`: The base URL of the API responsible for generating reports.
   - `ProcessIntervalMinutes`: How often the worker should check for pending statements.

## Database Schema Requirement

The service expects a table named `[GTMail].[dbo].[Profile]` with the following key columns:
- `CustomerNo` (Primary Key)
- `Email`, `DisplayName`, `Status` (Status 3 = Active)
- `Daily`, `Weekly`, `Monthly` (Bit/Int flags)
- `WeeklyOpt`, `MonthlyOpt` (String options for specific days)
- `LastsentDaily`, `LastsentWeekly`, `LastsentMonthly` (DateTime)

## Running the Application

To run the application in the development environment:
    dotnet run

To publish for production:
    dotnet publish -c Release -o ./publish

## Project Structure

- `Program.cs`: Entry point and DI container configuration.
- `Worker.cs`: The background service loop.
- `Services/StatementProcessor.cs`: Orchestrates the business logic of checking, fetching, and sending.
- `Services/ReportService.cs`: Handles HTTP communication with the Report API.
- `Services/EmailService.cs`: Handles SMTP communication via MailKit.
- `Models/CustomerProfile.cs`: EF Core entity mapping for the customer database.
- `Data/GTMailDbContext.cs`: Database context for SQL Server access.

## Troubleshooting

- **SMTP Errors**: Ensure the server `10.230.1.38` is reachable from the host and allows relaying from the service IP.
- **API Failures**: Verify `CoreBaseUrl` is correct and the endpoint `ExportFile/ReportFile` accepts the parameters `customerNo`, `format`, and `accounts`.
- **Database Connection**: Check if the SQL user has `SELECT` and `UPDATE` permissions on the `Profile` table.

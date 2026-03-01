using GTBStatementService.Data;
using GTBStatementService.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

class StatementRepository : IStatementRepository
{
    private readonly ILogger<StatementRepository> _logger;
    private readonly GTMailDbContext _context;
    private readonly string _connString;
    private readonly string _connectSQL;
    private string _tryCount;

    public StatementRepository(ILogger<StatementRepository> logger, IConfiguration connString, GTMailDbContext context)
    {
        _logger = logger;
        _connString = connString.GetConnectionString("OracleDb") ?? throw new ArgumentNullException("No finacle connection string specified");
        _connectSQL = connString.GetConnectionString("ConnectSQL") ?? throw new ArgumentNullException("No sql connection string specified");
        _context = context;
        _tryCount = connString["StatementSettings:TryCount"] ?? "3";
    }

    public async Task<List<StatementTransaction>> GetStatementsAsync(
    string accountNo, DateTime from, DateTime to)
    {
        using var conn = new OracleConnection(_connString);
        using var cmd = new OracleCommand("GET_STATEMENT", conn);

        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("p_account_no", OracleDbType.Varchar2).Value = accountNo;
        cmd.Parameters.Add("p_from_date", OracleDbType.Date).Value = from;
        cmd.Parameters.Add("p_to_date", OracleDbType.Date).Value = to;
        cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

        await conn.OpenAsync();

        using var reader = ((OracleRefCursor)cmd.Parameters["p_result"].Value).GetDataReader();

        var list = new List<StatementTransaction>();
        while (await reader.ReadAsync())
        {
            list.Add(new StatementTransaction
            {
                Date = reader.GetDateTime(0),
                Description = reader.GetString(1),
                Debit = reader.GetDecimal(2),
                Credit = reader.GetDecimal(2),
                Balance = reader.GetDecimal(3)
            });
        }
        return list;
    }

    public List<StatementTransaction> GetStatements(
        string accountNo, DateTime from, DateTime to)
    {
        const string sqlResource =
            "Notifier.Assets.GetAccountTransactions.sql";

        var sql = SqlLoader.Load(sqlResource);

        using var conn = new OracleConnection(_connString);
        using var cmd = new OracleCommand(sql, conn);

        cmd.Parameters.Add(":NUBAN", OracleDbType.Varchar2).Value = accountNo;
        cmd.Parameters.Add(":FROM_DATE", OracleDbType.Date).Value = from;
        cmd.Parameters.Add(":TO_DATE", OracleDbType.Date).Value = to;

        conn.Open();
        using var reader = cmd.ExecuteReader();

        var list = new List<StatementTransaction>();
        while (reader.Read())
        {
            list.Add(new StatementTransaction
            {
                Date = reader.GetDateTime(0),
                Description = reader.GetString(1),
                Debit = reader.GetDecimal(2),
                Credit = reader.GetDecimal(2),
                Balance = reader.GetDecimal(3)
            });
        }

        return list;
    }

    public async Task<Statement> GetStatementByIdAsync(int id)
    {
        return await _context.Statements.FindAsync(id);
    }

    //public async Task<IEnumerable<Statement>> GetAllStatementsAsync()
    //{
    //    return await _context.Statements.ToListAsync();
    //}

    public async Task AddStatementAsync(Statement statement)
    {
        await _context.Statements.AddAsync(statement);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateStatementAsync(Statement statement)
    {
        _context.Statements.Update(statement);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteStatementAsync(int id)
    {
        var statement = await GetStatementByIdAsync(id);
        if (statement != null)
        {
            _context.Statements.Remove(statement);
            await _context.SaveChangesAsync();
        }
    }

    public List<StatementTransaction> GetAccountStatements(string accountList, DateTime from, DateTime to)
    {
        //var list = accountList.Split(",")[];
        //accountList.Select(accountNo => GetStatements(
        //        accountNo, from, to));


        return GetStatements(
                accountList, from, to);
    }


    public List<BankStatement> GetCustomerAccounts(string customerId)
    {
        var results = new List<BankStatement>();

        string query = @"
            SELECT foracid
            FROM tbaadm.gam
            WHERE cif_id = :CUSTOMER_ID
              AND del_flg = 'N'";

        using (var connection = new OracleConnection("Your_Connection_String"))
        using (var command = new OracleCommand(query, connection))
        {
            command.BindByName = true;

            command.Parameters.Add(new OracleParameter("CUSTOMER_ID", OracleDbType.Varchar2)
            {
                Value = customerId
            });

            connection.Open();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    results.Add(new BankStatement
                    {
                        AccountNumber = reader["foracid"].ToString()
                    });
                }
            }
        }

        return results;
    }

    public async Task<List<CustomerProfile>> GetCustomerProfileAsync()
    {
        var results = new List<CustomerProfile>();

        using (var connection = new SqlConnection(_connectSQL))
        using (var command = new SqlCommand("sp_GTMail_StatmentList", connection))
        {
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@TryCount", SqlDbType.Int)
                   .Value = _tryCount;

            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    results.Add(new CustomerProfile
                    {
                        Date = reader["Date"] as DateTime?,
                        Branch = Convert.ToInt32(reader["Branch"]),
                        UserID = Convert.ToInt32(reader["UserID"]),
                        CustomerNo = reader["CustomerNo"]?.ToString(),
                        Status = Convert.ToInt32(reader["Status"]),
                        StmtType = Convert.ToInt32(reader["StmtType"]),
                        ActType = Convert.ToInt32(reader["ActType"]),
                        CurType = Convert.ToInt32(reader["CurType"]),
                        FromDate = reader["FromDate"] as DateTime?,
                        ToDate = reader["ToDate"] as DateTime?,
                        TryCount = Convert.ToInt32(reader["TryCount"]),
                        Index = Convert.ToInt32(reader["Index"]),
                        FinacleAccount = reader["FinacleAccount"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        DisplayName = reader["DisplayName"]?.ToString(),
                        ExportFile = reader["ExportFile"]?.ToString(),
                        ExportFormat = Convert.ToInt32(reader["ExportFormat"]),
                        DefaultExt = reader["DefaultExt"]?.ToString()
                    });
                }
            }
        }

        return results;
    }

    public async Task<CustomerProfile> GetCustomerProfileAsync(string customerId)
    {
        await _context.Profiles
                .Where(p => p.Status == 3 && !string.IsNullOrEmpty(p.Email))
                .ToListAsync();
        return new CustomerProfile
        {
            CustomerNo = customerId,
            DisplayName = "John Doe",
            Email = "  "
        };
    } 
}
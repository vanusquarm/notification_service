using GTBStatementService.Data;
using Microsoft.Identity.Client;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

class StatementRepository: IStatementRepository
{
    private readonly GTMailDbContext _context;
    private readonly string _connString;

    public StatementRepository(IConfiguration connString, GTMailDbContext context)
    {
        _connString = connString.GetConnectionString("OracleDb") ?? throw new ArgumentNullException("No finacle connection string specified");
        _context = context;
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

        cmd.Parameters.Add(":acc", OracleDbType.Varchar2).Value = accountNo;
        cmd.Parameters.Add(":fromDate", OracleDbType.Date).Value = from;
        cmd.Parameters.Add(":toDate", OracleDbType.Date).Value = to;

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
}
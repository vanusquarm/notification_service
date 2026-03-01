namespace GTBStatementService.Data
{
    public interface IStatementRepository
    {
        List<StatementTransaction> GetAccountStatements(string accountList, DateTime from, DateTime to);
    }
}
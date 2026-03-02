using GTBStatementService.Models;

namespace GTBStatementService.Data
{
    public interface IStatementRepository
    {
        List<StatementTransaction> GetAccountStatements(string accountList, DateTime from, DateTime to);
        Task<List<CustomerProfile>> GetCustomerProfileAsync();
        List<Account> GetCustomerAccounts(string customerId);
        public List<StatementTransaction> GetAccountTransactions(
            string accountNo, DateTime from, DateTime to);
    }
}
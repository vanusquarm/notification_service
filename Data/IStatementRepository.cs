using GTBStatementService.Models;

namespace GTBStatementService.Data
{
    public interface IStatementRepository
    {
        List<StatementTransaction> GetAccountStatements(string accountList, DateTime from, DateTime to);
        Task<List<CustomerProfile>> GetCustomerProfileAsync();
         List<BankStatement> GetCustomerAccounts(string customerId);

    }
}
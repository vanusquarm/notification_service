using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTBStatementService.Data.Mock
{
    public static class MockFactory
    {
        public static BankStatement CreateMock()
        {
            return new BankStatement
            {
                CustomerName = "John Doe",
                AccountNumber = "1234567890",
                AccountType = "Savings",
                Currency = "USD",
                PeriodFrom = new DateTime(2025, 1, 1),
                PeriodTo = new DateTime(2025, 1, 31),
                Transactions = new List<StatementTransaction>
            {
                new StatementTransaction
                {
                    Date = new DateTime(2025, 1, 2),
                    Description = "Opening Balance",
                    Debit = 0,
                    Credit = 0,
                    Balance = 1000m
                },
                new StatementTransaction
                {
                    Date = new DateTime(2025, 1, 5),
                    Description = "ATM Withdrawal",
                    Debit = 200m,
                    Credit = 0,
                    Balance = 800m
                },
                new StatementTransaction
                {
                    Date = new DateTime(2025, 1, 10),
                    Description = "Salary Deposit",
                    Debit = 0,
                    Credit = 1500m,
                    Balance = 2300m
                },
                new StatementTransaction
                {
                    Date = new DateTime(2025, 1, 20),
                    Description = "Online Transfer",
                    Debit = 300m,
                    Credit = 0,
                    Balance = 2000m
                }
            }
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTBStatementService.Models
{
    public class Account
    {
        public Account() {}

        public Account(string accountId) 
        { 
            AccountId = accountId; 
            AccountName = string.Empty;
            AccountType = string.Empty;
            Currency = string.Empty;    
        }
        public required string AccountId { get; set; }
        public required string AccountName { get; set; }
        public required string AccountType { get; set; }
        public required string Currency { get; set; }
    }
}

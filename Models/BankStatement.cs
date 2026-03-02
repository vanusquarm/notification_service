public class BankStatement
{
    public string BankName { get; set; } = "GTBank Liberia";
    public string? AccountName { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountType { get; set; }
    public string? Currency { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public List<StatementTransaction> Transactions { get; set; } = new();
    public string BranchName { get; set; }
    public string Address { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
}

public class StatementTransaction
{
    public DateTime TransactionDate { get; set; }
    public DateTime TransactionValueDate { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
    public string Remarks { get; internal set; }
}

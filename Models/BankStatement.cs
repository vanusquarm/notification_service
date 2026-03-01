public class BankStatement
{
    public string BankName { get; set; }
    public string CustomerName { get; set; }
    public string AccountNumber { get; set; }
    public string AccountType { get; set; }
    public string Currency { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }
    public List<StatementTransaction> Transactions { get; set; } = new();
}

public class StatementTransaction
{
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
}

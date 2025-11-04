namespace BankApp.Domain;

/// <summary>
/// Represents a single transaction connected to a specific account.
/// </summary>
public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid? CounterpartyAccountId { get; set; }
    public string? Note { get; set; }

    // Constructor used when creating a new transaction. 
    public Transaction(Guid accountId, TransactionType type, decimal amount, decimal balanceAfter ,Guid? counterpartyAccountId, string? note)
    {
        AccountId = accountId;
        Type = type;
        Amount = amount;
        BalanceAfter = balanceAfter;
        CounterpartyAccountId = counterpartyAccountId;
        Note = note;
        Date = DateTime.Now;
    }
}
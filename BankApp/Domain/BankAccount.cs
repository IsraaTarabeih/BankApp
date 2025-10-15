namespace BankApp.Domain;

public class BankAccount : IBankAccount
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Name { get; private set; } = string.Empty;
    public AccountType AccountType { get; private set; }
    public string Currency { get; private set; } = string.Empty;

    public decimal Balance { get; private set; } 

    public DateTime LastUpdated { get; private set; }


    public BankAccount() { }
    public BankAccount(string name, AccountType accountType, string currency, decimal initialbalance)
    {
        Id = Guid.NewGuid();
        Name = name;
        AccountType = accountType;
        Currency = currency;
        Balance = initialbalance;
        LastUpdated = DateTime.Now;
    }

    [System.Text.Json.Serialization.JsonConstructor]
    public BankAccount(Guid id, string name, AccountType accountType, string currency, decimal balance, DateTime lastUpdated)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = name ?? string.Empty;
        AccountType = accountType;
        Currency = currency ?? string.Empty;
        Balance = balance;
        LastUpdated = lastUpdated == default ? DateTime.Now : lastUpdated;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0) return;
        Balance += amount;
        LastUpdated = DateTime.Now;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0 || amount > Balance) return;
        Balance -= amount;
        LastUpdated = DateTime.Now;
    }
}

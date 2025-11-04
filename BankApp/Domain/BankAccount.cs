namespace BankApp.Domain;

/// <summary>
/// BankAccount domain, represents a bank account and handles its transactions. 
/// </summary>
public class BankAccount : IBankAccount
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public AccountType AccountType { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public decimal Balance { get; private set; } 
    public DateTime LastUpdated { get; private set; }

    // Constructor used when creating a new account. It saves the given values into the account´s fields.
    public BankAccount(string name, AccountType accountType, string currency, decimal initialbalance)
    {
        Id = Guid.NewGuid();
        Name = name;
        AccountType = accountType;
        Currency = currency;
        Balance = initialbalance;
        LastUpdated = DateTime.Now;
    }

    // Used when loading the account from storage.
    [JsonConstructor]
    public BankAccount(Guid id, string name, AccountType accountType, string currency, decimal balance, DateTime lastUpdated)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = name ?? string.Empty;
        AccountType = accountType;
        Currency = currency ?? string.Empty;
        Balance = balance;
        LastUpdated = lastUpdated == default ? DateTime.Now : lastUpdated;
    }

    // Adds the given amount to the balance and updates the timestamp - "LastUpdated".
    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Belopp måste vara större än 0.");
        Balance += amount;
        LastUpdated = DateTime.Now;
    }

    // Subtracts the given amount from the balance and updates the timestamp -  "LastUpdated".
    public void Withdraw(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Belopp måste vara större än 0 kr.");
        if (amount > Balance) throw new InvalidOperationException("Otillräckligt saldo.");
        Balance -= amount;
        LastUpdated = DateTime.Now;
    }
}
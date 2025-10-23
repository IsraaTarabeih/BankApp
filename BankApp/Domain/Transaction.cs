namespace BankApp.Domain;

public class Transaction // Modell med egenskaper för en transaktion som ska sparas. 
{
    public Guid Id { get; set; } = Guid.NewGuid(); // Unik identifierare.
    public Guid AccountId { get; set; } // Vilket konto transaktionen gäller.
    public DateTime Date { get; set; } // Tidpunkt (tilldelas längre ned i konstruktorn. 
    public TransactionType Type { get; set; } // Vilken typ av transaktion (överföring/insättning/uttag).
    public decimal Amount { get; set; } // Bellop.
    public decimal BalanceAfter { get; set; } // Kontots saldo efter transaktionen är gjord. 
    public Guid? CounterpartyAccountId { get; set; } // Motpartens kontonamn. 
    public string? Note { get; set; } // Valfri anteckning. 

    public Transaction() { } // Parameterlös konstuktor behövs för serialisering. 

    // "Bekväm" konstruktor: sätter allt utom Id(som får default) och sätter Date till nu. 
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

    [System.Text.Json.Serialization.JsonConstructor] // Används när objekt återskapas från lagring. Alltså finns konton man lagrat kvar även efter man stängt ner programmet. 
    public Transaction(Guid id, Guid accountId, DateTime date, TransactionType type, decimal amount, decimal balanceAfter, Guid? counterpartyAccountId, string? note)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id; // Om id är tomt - generera nytt. 
        AccountId = accountId;
        Date = date == default ? DateTime.Now : date; // Om date är default - sätt nu. 
        Type = type;
        Amount = amount;
        BalanceAfter = balanceAfter;
        CounterpartyAccountId = counterpartyAccountId;
        Note = note;
        // Resten tas som det är. 
    }
}

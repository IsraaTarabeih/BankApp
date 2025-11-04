namespace BankApp.Domain;

/// <summary>
/// Defines the different types of transactions.
/// </summary> 
public enum TransactionType 
{
    Insättning = 0,
    Uttag = 1,
    ÖverföringIn = 2,
    ÖverföringUt = 3
}
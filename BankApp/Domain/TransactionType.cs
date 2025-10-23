namespace BankApp.Domain;

public enum TransactionType // Definierar tillåtna transaktionstyper. 
{
    Insättning = 0, // Representeras internt som heltalet 0. Det är bra att tilldela siffror om man har planer på att lägga till fler funktioner framöver. 
    Uttag = 1,
    ÖverföringIn = 2,
    ÖverföringUt = 3
}

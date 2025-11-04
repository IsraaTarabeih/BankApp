namespace BankApp.Interface;

/// <summary>
/// Defines the structure for saving and loading accounts and transactions.
/// </summary>
public interface IStorageService
{
    // Loads all saved bank accounts and returns a list of them.
    Task<List<BankAccount>> LoadAccountsAsync();

    // Saves the provided list of bank accounts to storage.
    Task SaveAccountsAsync (List<BankAccount> accounts);

    // Loads all saved transactions and returns a list of them.
    Task<List<Transaction>> LoadTransactionsAsync();

    // Saves the provided list of transactions to storage.
    Task SaveTransactionsAsync (List<Transaction> transactions);

    // Clears all stored data from the storage.
    Task ClearAsync();
}
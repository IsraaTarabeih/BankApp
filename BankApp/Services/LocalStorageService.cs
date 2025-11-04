namespace BankApp.Services;

/// <summary>
/// Handles saving and loading data from the browser´s local storage.
/// Used to store accounts and transactions as JSON.
/// </summary>
public class LocalStorageService : IStorageService
{
    private const string Key = "accounts_v6"; // Nyckel där kontolistan lagras som JSON i LocalStorage.
    private const string TransactionsKey = "transactions_v1"; // Nyckel där transaktionslistan lagras. 
    private readonly ILocalStorageService _localStorage; // Håller kopplingen till LocalStorage så vi kan spara och läsa data i webbläsaren.  

    // Initializes the local storage service.
    public LocalStorageService(ILocalStorageService localStorage) => _localStorage = localStorage; // Får in localStorage-tjänsten automatisk (Dependency Injection) och sparar den i fältet.

    // Loads all saved bank accounts from local storage. 
    public async Task<List<BankAccount>> LoadAccountsAsync() =>

        await _localStorage.GetItemAsync<List<BankAccount>>(Key) ?? new List<BankAccount>(); // Hämtar kontolistan från LocalStorage, eller skapar en tom lista om ingen finns sparad.

    // Saves all bank accounts to local storage.  
    public async Task SaveAccountsAsync(List<BankAccount> accounts) =>
    
        await _localStorage.SetItemAsync(Key, accounts); // Gör om lista till text (JSON) och sparar den i webbläsarens localStorage. 

    // Loads all saved transactions from local storage. 
    public async Task<List<Transaction>> LoadTransactionsAsync() =>

        await _localStorage.GetItemAsync<List<Transaction>>(TransactionsKey) ?? new List<Transaction>(); // Läs transaktioner (eller tom lista). 

    // Saves all transactions to local storage. 
    public async Task SaveTransactionsAsync(List<Transaction> transactions) =>

        await _localStorage.SetItemAsync(TransactionsKey, transactions); // Sparar alla transaktioner i webbläsaren så de finnas kvar nästa gång man öppnar appen.
    
    // Clears all stored account and transaction data from local storage. 
    public async Task ClearAsync()
    {
        await _localStorage.RemoveItemAsync(Key); // Ta bort kontolistan i localStorage.
        await _localStorage.RemoveItemAsync(TransactionsKey); // Ta bort transaktionslistan i localStorage.
    }
}
namespace BankApp.Services;

/// <summary>
/// Handles saving and loading data from the browser´s local storage.
/// Used to store accounts and transactions as JSON.
/// </summary>
public class LocalStorageService : IStorageService
{
    private const string Key = "accounts_v6"; 
    private const string TransactionsKey = "transactions_v1";  
    private readonly ILocalStorageService _localStorage;   

    // Initializes the local storage service.
    public LocalStorageService(ILocalStorageService localStorage) => _localStorage = localStorage; 

    // Loads all saved bank accounts from local storage. 
    public async Task<List<BankAccount>> LoadAccountsAsync() =>

        await _localStorage.GetItemAsync<List<BankAccount>>(Key) ?? new List<BankAccount>(); 

    // Saves all bank accounts to local storage.  
    public async Task SaveAccountsAsync(List<BankAccount> accounts) =>
    
        await _localStorage.SetItemAsync(Key, accounts);  

    // Loads all saved transactions from local storage. 
    public async Task<List<Transaction>> LoadTransactionsAsync() =>

        await _localStorage.GetItemAsync<List<Transaction>>(TransactionsKey) ?? new List<Transaction>();

    // Saves all transactions to local storage. 
    public async Task SaveTransactionsAsync(List<Transaction> transactions) =>

        await _localStorage.SetItemAsync(TransactionsKey, transactions); 
    
    // Clears all stored account and transaction data from local storage. 
    public async Task ClearAsync()
    {
        await _localStorage.RemoveItemAsync(Key); 
        await _localStorage.RemoveItemAsync(TransactionsKey); 
    }
}
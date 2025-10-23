namespace BankApp.Services;

public class LocalStorageService : IStorageService
{
    private const string Key = "accounts_v6";
    private const string TransactionsKey = "transactions_v1";
    private readonly ILocalStorageService _localStorage;

    public LocalStorageService(ILocalStorageService localStorage) => _localStorage = localStorage;
    

    public async Task<List<BankAccount>> LoadAccountsAsync() =>
        
        await _localStorage.GetItemAsync<List<BankAccount>>(Key) ?? new List<BankAccount>();
    
    public async Task SaveAccountsAsync(List<BankAccount> accounts) =>
    
        await _localStorage.SetItemAsync(Key, accounts);

    public async Task<List<Transaction>> LoadTransactionsAsync() =>

        await _localStorage.GetItemAsync<List<Transaction>>(TransactionsKey) ?? new List<Transaction>(); 

    public async Task SaveTransactionsAsync(List<Transaction> transactions) =>

        await _localStorage.SetItemAsync(TransactionsKey, transactions);
    
    public async Task ClearAsync()
    {
        await _localStorage.RemoveItemAsync(Key);
        await _localStorage.RemoveItemAsync(TransactionsKey);
    }
}

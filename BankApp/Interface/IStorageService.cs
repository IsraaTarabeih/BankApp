namespace BankApp.Interface;

public interface IStorageService
{
    Task<List<BankAccount>> LoadAccountsAsync();
    Task SaveAccountsAsync (List<BankAccount> accounts);
    Task<List<Transaction>> LoadTransactionsAsync();
    Task SaveTransactionsAsync (List<Transaction> transactions);
    Task ClearAsync();
}

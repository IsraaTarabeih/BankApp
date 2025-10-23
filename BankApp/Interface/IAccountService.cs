namespace BankApp.Interface;

public interface IAccountService
{
   
    Task<IBankAccount> CreateBankAccountAsync(string name, AccountType accountType, string currency, decimal initialBalance);
    Task<List<IBankAccount>>GetAccountsAsync();
    Task DeleteAccountAsync(Guid id);

    Task DepositAsync(Guid accountId, decimal amount, string? note = null);
    Task WithdrawAsync(Guid accountId, decimal amount, string? note = null);
    Task TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount, string? note = null);
    

    Task<List<Transaction>> GetTransactionsAsync(Guid? accountId = null);
}

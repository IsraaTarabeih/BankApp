namespace BankApp.Interface
{
    public interface IAccountService
    {
       
        Task<IBankAccount> CreateBankAccountAsync(string name, AccountType accountType, string currency, decimal initialBalance);
        Task<List<IBankAccount>>GetAccountsAsync();
       
    }
}

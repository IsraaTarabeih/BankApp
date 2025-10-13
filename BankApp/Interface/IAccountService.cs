namespace BankApp.Interface
{
    public interface IAccountService
    {
        IBankAccount CreateBankAccount(string name, AccountType accountType, string currency, decimal initialBalance);
        List<IBankAccount> GetAccounts();
       
    }
}

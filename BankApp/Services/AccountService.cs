namespace BankApp.Services
{
    public class AccountService : IAccountService
    {
        private readonly List<IBankAccount> _accounts = new();
        
        public IBankAccount CreateBankAccount(string name, AccountType accountType, string currency, decimal initialBalance)
        {
            var account = new BankAccount(name, accountType, currency, initialBalance);
                _accounts.Add(account);
            return account;
        }
        public List<IBankAccount> GetAccounts() => _accounts;

    }
}

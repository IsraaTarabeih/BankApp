namespace BankApp.Services
{
    public class AccountService : IAccountService
    {
        private readonly IStorageService _storage;
        private List<IBankAccount> _accounts = new();
        private bool _loaded;

        public AccountService(IStorageService storage)
        {
            _storage = storage;
        }

        public async Task<IBankAccount> CreateBankAccountAsync(string name, AccountType accountType,string currency, decimal initialBalance)
        {
            await EnsureLoadedAsync();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Namn och valuta krävs.");

            var account = new BankAccount(name,accountType, currency, initialBalance);
            _accounts.Add(account);
            await PersistAsync();
            return account;
        }

        public async Task<List<IBankAccount>> GetAccountsAsync()
        {
            await EnsureLoadedAsync();
            return new List<IBankAccount>(_accounts);
        }
        private async Task EnsureLoadedAsync()
        {
            if (_loaded) return;
            var loaded = await _storage.LoadAccountsAsync();
            _accounts = loaded.Cast<IBankAccount>().ToList();
            _loaded = true;
        }
        private async Task PersistAsync()
        {
            var concrete = _accounts.OfType<BankAccount>().ToList();
            await _storage.SaveAccountsAsync(concrete);
        }

    }
}

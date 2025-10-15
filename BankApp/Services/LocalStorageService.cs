namespace BankApp.Services
{
    public class LocalStorageService : IStorageService
    {
        private const string Key = "accounts_V3";
        private readonly ILocalStorageService _localStorage;

        public LocalStorageService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<List<BankAccount>> LoadAccountsAsync()
        {
            return await _localStorage.GetItemAsync<List<BankAccount>>(Key) ?? new List<BankAccount>();
        }
        public async Task SaveAccountsAsync(List<BankAccount> accounts) 
        {
            await _localStorage.SetItemAsync(Key, accounts);
        }
        public async Task ClearAsync()
        {
            await _localStorage.RemoveItemAsync(Key);
        }
    }
}

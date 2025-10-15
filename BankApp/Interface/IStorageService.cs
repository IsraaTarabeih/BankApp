namespace BankApp.Interface
{
    public interface IStorageService
    {
        Task<List<BankAccount>> LoadAccountsAsync();
        Task SaveAccountsAsync (List<BankAccount> accounts);
        Task ClearAsync();
    }
}

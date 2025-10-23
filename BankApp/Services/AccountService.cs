namespace BankApp.Services;

public class AccountService : IAccountService
{
    private readonly IStorageService _storage;
    private readonly ILogger<AccountService> _logger;
    private List<IBankAccount> _accounts = new();
    private List<Transaction> _transactions = new();
    private bool _loaded;

    public AccountService(IStorageService storage, ILogger<AccountService> logger)
    {
        _storage = storage;
        _logger = logger;
    }
    

    public async Task<IBankAccount> CreateBankAccountAsync(string name, AccountType accountType,string currency, decimal initialBalance)
    {
        await EnsureLoadedAsync();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Namn och valuta krävs.");

        var account = new BankAccount(name,accountType, currency, initialBalance);
        _accounts.Add(account);
        _logger.LogInformation("Konto skapat: {Name} ({Type}) med startsaldo {Balance} {Currency}", 
            account.Name, account.AccountType, initialBalance, account.Currency);

        _transactions.Add(new Transaction(
            account.Id,
            TransactionType.Insättning,
            initialBalance,
            account.Balance,
            null,
            "Startsaldo"));

        await PersistAsync();
        return account;
    }

    public async Task<List<IBankAccount>> GetAccountsAsync()
    {
        await EnsureLoadedAsync();
        return _accounts.ToList();
    }
    public async Task DeleteAccountAsync(Guid id)
    {
        await EnsureLoadedAsync();

        var match = _accounts.FirstOrDefault(a => a.Id == id);
        if (match == null) return;

        _accounts.Remove(match);
        _transactions.RemoveAll(t => t.AccountId == id);
        await PersistAsync();
        _logger.LogInformation("Konto raderat: {AccountId}", id);
    }

    public async Task DepositAsync(Guid accountId, decimal amount, string? note = null)
    {
        await EnsureLoadedAsync();
        if (amount <= 0) throw new ArgumentException("Belopp måste vara större än 0 kr.");
        var acc = _accounts.FirstOrDefault(a => a.Id == accountId) as BankAccount;
        if (acc is null) throw new InvalidOperationException("Konto hittades inte.");


        acc.Deposit(amount);
        _transactions.Add(new Transaction(acc.Id, TransactionType.Insättning, amount, acc.Balance, null, note));
        await PersistAsync();
        _logger.LogInformation("Insättning {Amount} till konto {AccountID}. Nytt saldo {Balance}", amount, accountId, acc.Balance);
    }

    public async Task WithdrawAsync(Guid accountId, decimal amount, string? note = null)
    {
        await EnsureLoadedAsync();
        if (amount <= 0) throw new ArgumentException("Belopp måste vara större än 0 kr.");
        var acc = _accounts.FirstOrDefault(a => a.Id == accountId) as BankAccount;
        if (acc is null) throw new InvalidOperationException("Konto hittades inte.");
        if (amount > acc.Balance) throw new InvalidOperationException("Otillräckligt saldo.");

        acc.Withdraw(amount);
        _transactions.Add(new Transaction(acc.Id, TransactionType.Uttag, amount, acc.Balance, null, note));
        await PersistAsync();
        _logger.LogInformation("Uttag {Amount} från konto {AccountId}. Nytt saldo {Balance}", amount, accountId, acc.Balance);
    }

    public async Task TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount, string? note = null)
    {
        await EnsureLoadedAsync();
        if (fromAccountId == toAccountId) throw new ArgumentException("Välj två olika konton.");
        if (amount <= 0) throw new ArgumentException("Belopp måste vara större än 0 kr.");

        var from = _accounts.FirstOrDefault(a => a.Id == fromAccountId) as BankAccount;
        var to = _accounts.FirstOrDefault(a => a.Id == toAccountId) as BankAccount;
        if (from is null || to is null) throw new InvalidOperationException("Konto hittades inte.");
        if (amount > from.Balance) throw new InvalidOperationException("Otillräckligt saldo.");


        from.Withdraw(amount);
        _transactions.Add(new Transaction(from.Id, TransactionType.ÖverföringUt, amount, from.Balance, to.Id, note));

        to.Deposit(amount);
        _transactions.Add(new Transaction(to.Id, TransactionType.ÖverföringIn, amount, to.Balance, from.Id, note));

        await PersistAsync();
        _logger.LogInformation("Överföring {Amount} från {From} till {To}", amount, fromAccountId, toAccountId);
    }

    public async Task<List<Transaction>> GetTransactionsAsync(Guid? accountId = null)
    {
        await EnsureLoadedAsync();
        var list = accountId is null ? _transactions : _transactions.Where(t => t.AccountId == accountId).ToList();
        return list.OrderByDescending(t => t.Date).ToList();
    }
    private async Task EnsureLoadedAsync()
    {
        if (_loaded) return;
        var loadedAccounts = await _storage.LoadAccountsAsync();
        _accounts = loadedAccounts.Cast<IBankAccount>().ToList();
        _transactions = await _storage.LoadTransactionsAsync();
        _loaded = true;
    }
    private async Task PersistAsync()
    {
        var concrete = _accounts.OfType<BankAccount>().ToList();
        await _storage.SaveAccountsAsync(concrete);
        await _storage.SaveTransactionsAsync(_transactions);
    }

}

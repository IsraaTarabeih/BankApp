namespace BankApp.Services;

/// <summary>
/// Handles all logic for accounts and transactions.
/// Uses local storage to save data and logs important actions.
/// Creates/deletes accounts, return account and transaction lists. 
/// Executes deposits, withdraws, transfers (with validation).
/// Applies periodiv auto-interest via timer and raises InterestApplied. 
/// Exports/imports data as JSON (replace or merge).
/// </summary>
public class AccountService : IAccountService
{
    private readonly IStorageService _storage;
    private readonly ILogger<AccountService> _logger; 
    private List<IBankAccount> _accounts = new(); 
    private List<Transaction> _transactions = new(); 
    private bool _loaded; 
    public event EventHandler<InterestAppliedEventArgs>? InterestApplied;

    // Sets up the account service and connects it to storage and logging.
    public AccountService(ILogger<AccountService> logger, IStorageService storage)
    {
        _storage = storage; 
        _logger = logger; 
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    // Creates a new bank account, saves it and its details, and adds it to the list of accounts.
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

    // Gets all saved bank accounts.
    public async Task<List<IBankAccount>> GetAccountsAsync()
    {
        await EnsureLoadedAsync();
        return _accounts.ToList(); 
    }

    // Deletes a selected account and removes all its transactions. 
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

    // Adds money to an account and saves the transaction. 
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

    // Withdraws money from an account and saves the transaction. 
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

    // Moves money from one account to another and saves both transactions. 
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

    // Gets all saved transactions, or only those for a specific account if an ID is provided. 
    public async Task<List<Transaction>> GetTransactionsAsync(Guid? accountId = null)
    {
        await EnsureLoadedAsync();
        var list = accountId is null ? _transactions : _transactions.Where(t => t.AccountId == accountId).ToList();
        return list.OrderByDescending(t => t.Date).ToList(); 
    }

    // Creates a JSON string containing all accounts and transactions for export or backup.
    public async Task<string> ExportJsonAsync()
    {
        await EnsureLoadedAsync();

        var export = new ExportModel
        {
            Accounts = _accounts.Select(a => new AccountDto
            {
                Id = a.Id,
                Name = a.Name,
                AccountType = a.AccountType.ToString(),
                Currency = a.Currency,
                Balance = a.Balance,
                LastUpdated = a.LastUpdated
            }).ToList(),

            Transactions = _transactions
            .OrderBy(t => t.Date)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Date = t.Date,
                Amount = t.Amount,
                BalanceAfter = t.BalanceAfter,
                AccountId = t.AccountId,
                ToAccountId = t.CounterpartyAccountId,
                TransactionType = t.Type.ToString(),
                Note = t.Note
            }).ToList(),
        };
        return JsonSerializer.Serialize(export, _jsonOptions);
    }

    // Reads and validates account and transaction data from a JSON file. 
    // Replaces existing data if "replaceExisting" is true and logs the reslut.
    // Returns any errors found during import.
    public async Task<List<string>> ImportJsonAsync(string json, bool replaceExisting = true)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(json))
        {
            errors.Add("Tom JSON.");
            return errors;
        }

        ExportModel? incoming;
        try
        {
            incoming = JsonSerializer.Deserialize<ExportModel>(json, _jsonOptions);
        }
        catch
        {
            errors.Add("Ogiltig JSON.");
            return errors;
        }
        if (incoming is null)
        {
            errors.Add("Ingen data i filen");
            return errors;
        }

        var accIds = incoming.Accounts.Select(a => a.Id).ToHashSet();
        if (incoming.Transactions.Any(t => !accIds.Contains(t.AccountId)))
        {
            errors.Add("Minst en transaktion pekar på ett konto som inte finns i JSON-filen.");
            return errors;
        }

        await EnsureLoadedAsync();

        if (replaceExisting)
        {
            _accounts = incoming.Accounts.Select< AccountDto, IBankAccount >(a =>
            new BankAccount(
                a.Id,
                a.Name,
                Enum.Parse<AccountType>(a.AccountType, ignoreCase: true),
                a.Currency,
                a.Balance,
                a.LastUpdated
                )
            ).ToList();

            _transactions = incoming.Transactions
                .Select(t => new Transaction(
                t.AccountId,
                Enum.Parse<TransactionType>(t.TransactionType, ignoreCase: true),
                t.Amount,
                t.BalanceAfter,
                t.ToAccountId,
                t.Note
                ))
            .OrderByDescending(x => x.Date)
             .ToList();
        }

        await PersistAsync();
        _logger.LogInformation("Import klar: {Accounts} konton, {Tx} transaktioner.", _accounts.Count, _transactions.Count);
        return errors;
    }

    // Loads accounts and transactions from local storage if they haven´t been loaded yet.
    // Ensures that the data is only loaded once, sorts accounts alphabetically and transactions by date.
    private async Task EnsureLoadedAsync()
    {
        if (_loaded) return;

        var loadedAccounts = await _storage.LoadAccountsAsync();
        var loadedTx = await _storage.LoadTransactionsAsync();
        
        _accounts = loadedAccounts.Cast<IBankAccount>().ToList();
        _transactions = loadedTx;

        _accounts = _accounts.OrderBy(a => a.Name).ToList();
        _transactions = _transactions.OrderByDescending(t => t.Date).ToList();
        
        _loaded = true;
    }
    
    // Saves all current accounts and transactions to local storage to keep data persistent between sessions.
    private async Task PersistAsync()
    {
        var concrete = _accounts.OfType<BankAccount>().ToList();
        await _storage.SaveAccountsAsync(concrete);
        await _storage.SaveTransactionsAsync(_transactions);
    }

    // Model used to store accounts and transactions during JSON export/import.
    private sealed class ExportModel
    {
        public List<AccountDto> Accounts { get; set; } = new();
        public List<TransactionDto> Transactions { get; set; } = new();
    }

    // Represents a simplified account model used for JSON export and import operations.
    private sealed class AccountDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string AccountType { get; set; } = "";
        public string Currency { get; set; } = "SEK";
        public decimal Balance { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    // Represents a simplified transaction model used for JSON export and import operations.
    private sealed class TransactionDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public Guid AccountId { get; set; }
        public Guid? ToAccountId { get; set; }
        public string TransactionType { get; set; } = "";
        public string? Note { get; set; }
    }

    // Timer used to automatically apply interest at regular intervals.
    private System.Threading.Timer? _interestTimer;

    // Starts a background timer that automatically applies interest to all accounts every 60 seconds.
    public void AutoInterestUpdates()
    {
        _interestTimer = new System.Threading.Timer(async _ =>
        {
            try
            {
                await AutoApplyInterestToAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid automatisk ränteuppdatering");
            }
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
    }

    // Calculates interest based on balance, annual rate, and number of days. 
    private static decimal CalcInterest(decimal balance, decimal annualRatePercent, int days) =>
        Math.Round(balance * (annualRatePercent /100m) * (days /365m), 2, MidpointRounding.ToEven);

    // Calculates and applies interest to all savings accounts, logs each transaction, triggers the interest event, and saves the updated data.
    private async Task AutoApplyInterestToAllAsync()
    {
        await EnsureLoadedAsync();

        const decimal annualRate = 2.5m;
        const int days = 30;

        foreach (var acc in _accounts.OfType<BankAccount>()
                                     .Where(a => a.AccountType == AccountType.Sparkonto))
        {
            var interest = CalcInterest(acc.Balance, annualRate, days);
            if (interest <= 0) continue;

            acc.Deposit(interest);
            _transactions.Add(new Transaction(
                acc.Id,
                TransactionType.Insättning,
                interest,
                acc.Balance,
                null,
                "Ränta"
            ));

            InterestApplied?.Invoke(this, new InterestAppliedEventArgs
            {
                AccountId = acc.Id,
                Amount = interest,
                When = DateTime.Now
            });
        }
        await PersistAsync();
    }
}
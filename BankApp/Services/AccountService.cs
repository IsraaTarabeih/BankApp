namespace BankApp.Services;

/// <summary>
/// Handles all logic for accounts and transactions.
/// Uses local storage to save data and logs important actions.
/// </summary>
public class AccountService : IAccountService
{
    private readonly IStorageService _storage;
    private readonly ILogger<AccountService> _logger; 
    private List<IBankAccount> _accounts = new(); // Håller alla konton i minnet medan appen kör. 
    private List<Transaction> _transactions = new(); // Håller alla transaktioner i minnet.
    private bool _loaded; // Dubbelkollar om de laddats info från LocalStorage ännu. 

    // Sets up the account service and connects it to storage and logging.
    public AccountService(IStorageService storage, ILogger<AccountService> logger)
    {
        _storage = storage; // Tjänst för att spara/läsa till/från LocalStorage.
        _logger = logger; // Enkel loggning av viktiga händelser. 
    }

    // Creates a new bank account, saves it and its details, and adds it to the list of accounts.
    public async Task<IBankAccount> CreateBankAccountAsync(string name, AccountType accountType,string currency, decimal initialBalance)
    {
        await EnsureLoadedAsync(); // Se till att listor i minnet är laddade från LocalStorage. 
        
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(currency)) // Om namn eller valuta (eller båda) står tomt eller mellanslag, skriv ut felmeddelande. 
            throw new ArgumentException("Namn och valuta krävs.");

        var account = new BankAccount(name,accountType, currency, initialBalance); // new BankAccount skapar ett nytt kontoobjekt i minnet. Den kör kontruktorn i klassen BankAccount.cs.
        _accounts.Add(account);  // Lägger till det skapade kontot i listan "_accounts". Den listan finns i AccountService.
        
        _logger.LogInformation("Konto skapat: {Name} ({Type}) med startsaldo {Balance} {Currency}", 
            account.Name, account.AccountType, initialBalance, account.Currency);

        _transactions.Add(new Transaction(
            account.Id,
            TransactionType.Insättning,
            initialBalance,
            account.Balance,
            null,
            "Startsaldo"));
        // Ovan skapar en transaktionspost som dokumenterar startsaldot. 

        await PersistAsync(); // Sparar hela listan (_accounts) till LocalStorage vid LocalStorageService.
                              // Så datan bevaras mellan sessioner. Gör de "permanent i webbläsaren". 
        return account;
    }

    // Gets all saved bank accounts.
    public async Task<List<IBankAccount>> GetAccountsAsync()
    {
        await EnsureLoadedAsync(); // Ladda från LocalStorage om det inte redan är gjort. 
        return _accounts.ToList(); // Returnerar en kopia så att anroparen inte kan ändra den interna lista direkt. 
    }

    // Deletes a selected account and removes all its transactions. 
    public async Task DeleteAccountAsync(Guid id)
    {
        await EnsureLoadedAsync(); // Säkerställer att jag jobbar mot den aktuella datan.

        var match = _accounts.FirstOrDefault(a => a.Id == id); 
        if (match == null) return; // Om kontot inte finns, inget att radera.

        _accounts.Remove(match); // Ta bort kontot ur listan. 
        _transactions.RemoveAll(t => t.AccountId == id); // Ta bort alla transaktioner kopplade till kontot. 
        await PersistAsync(); // Sparar förändringen till LocalStorage.  
        _logger.LogInformation("Konto raderat: {AccountId}", id);
    }

    // Adds money to an account and saves the transaction. 
    public async Task DepositAsync(Guid accountId, decimal amount, string? note = null)
    {
        await EnsureLoadedAsync(); // Jobba mot laddad data. 
        if (amount <= 0) throw new ArgumentException("Belopp måste vara större än 0 kr.");
        var acc = _accounts.FirstOrDefault(a => a.Id == accountId) as BankAccount; // Hämta kontot som ska få insättningen. 
        if (acc is null) throw new InvalidOperationException("Konto hittades inte."); // Säkerställ att kontot finns. 


        acc.Deposit(amount); // Ändrar saldot i domänobjektet (uppdaterar även LastUpdated). 
        _transactions.Add(new Transaction(acc.Id, TransactionType.Insättning, amount, acc.Balance, null, note)); // Logga händelsen. 
        await PersistAsync(); // Skriv ner ny status till LocalStorage. 
        _logger.LogInformation("Insättning {Amount} till konto {AccountID}. Nytt saldo {Balance}", amount, accountId, acc.Balance);
    }

    // Withdraws money from an account and saves the transaction. 
    public async Task WithdrawAsync(Guid accountId, decimal amount, string? note = null)
    {
        await EnsureLoadedAsync(); // Ladda data om nödvändigt. 
        if (amount <= 0) throw new ArgumentException("Belopp måste vara större än 0 kr.");
        var acc = _accounts.FirstOrDefault(a => a.Id == accountId) as BankAccount; // Hämta kontot.
        if (acc is null) throw new InvalidOperationException("Konto hittades inte."); // Kollar om kontot finns. 
        if (amount > acc.Balance) throw new InvalidOperationException("Otillräckligt saldo."); // Förhindrar övertrassering.

        acc.Withdraw(amount); // Ändrar saldot i domänobjektet (uppdaterar även LastUpdated)
        _transactions.Add(new Transaction(acc.Id, TransactionType.Uttag, amount, acc.Balance, null, note)); // Logga händelse.
        await PersistAsync(); // Spara ner till LocalStorage.
        _logger.LogInformation("Uttag {Amount} från konto {AccountId}. Nytt saldo {Balance}", amount, accountId, acc.Balance); 
    }

    // Moves money from one account to another and saves both transactions. 
    public async Task TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount, string? note = null)
    {
        await EnsureLoadedAsync(); // Ladda data om nödvändigt.
        if (fromAccountId == toAccountId) throw new ArgumentException("Välj två olika konton."); // Förhindrar att man väljer samma konto två gånger.
        if (amount <= 0) throw new ArgumentException("Belopp måste vara större än 0 kr."); // Belopp måste vara positivt. 

        var from = _accounts.FirstOrDefault(a => a.Id == fromAccountId) as BankAccount; // Källa
        var to = _accounts.FirstOrDefault(a => a.Id == toAccountId) as BankAccount; // Mottagare
        if (from is null || to is null) throw new InvalidOperationException("Konto hittades inte."); // Båda konton måste finnas.
        if (amount > from.Balance) throw new InvalidOperationException("Otillräckligt saldo."); // Källa får ej gå minus. 

        from.Withdraw(amount); // Minska saldo på från-kontot.
        _transactions.Add(new Transaction(from.Id, TransactionType.ÖverföringUt, amount, from.Balance, to.Id, note)); // Logga utgående del. 
        
        to.Deposit(amount); // Öka saldo på till-kontot. 
        _transactions.Add(new Transaction(to.Id, TransactionType.ÖverföringIn, amount, to.Balance, from.Id, note)); // Logga inkommande del.

        await PersistAsync(); // Sparar båda kontoändringar + transaktioner till LocalStorage.
        _logger.LogInformation("Överföring {Amount} från {From} till {To}", amount, fromAccountId, toAccountId);
    }

    // Gets all saved transactions, or only those for a specific account if an ID is provided. 
    public async Task<List<Transaction>> GetTransactionsAsync(Guid? accountId = null)
    {
        await EnsureLoadedAsync(); // Ladda transaktioner om de inte laddats än.
        var list = accountId is null ? _transactions : _transactions.Where(t => t.AccountId == accountId).ToList(); // Skrollar bland alla konton för att komma åt det "valda". 
        return list.OrderByDescending(t => t.Date).ToList(); // Senaste uppdatering först. 
    }

    // Loads accounts and transactions from local storage if they are not already loaded. 
    private async Task EnsureLoadedAsync()
    {
        if (_loaded) return; // Om de redan laddat från LocalStorage, gör inget igen.
        var loadedAccounts = await _storage.LoadAccountsAsync(); // Hämta konton från LocalStorage. 
        _accounts = loadedAccounts.Cast<IBankAccount>().ToList(); // Spara i in-memory-lista (som IBankAccount).
        _transactions = await _storage.LoadTransactionsAsync(); // Hämtar transaktioner från LocalStorage.
        _loaded = true; // Markera att vi nu är laddade.
    }

    // Saves all accounts and transactions to local storage. 
    private async Task PersistAsync()
    {
        var concrete = _accounts.OfType<BankAccount>().ToList(); // Gör om interfacelistan till konkreta BankAccount-objekt (krävs för serialisering). 

        await _storage.SaveAccountsAsync(concrete); // Skriv kontolistan till LocalStorage.
        await _storage.SaveTransactionsAsync(_transactions); // Skriv transaktionslistan till LocalStorage. 
    }
}
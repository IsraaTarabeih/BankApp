namespace BankApp.Interface;

/// <summary>
/// Defines all actions that can be done with bank accounts.
/// Includes creating, deleting, depositing, withdrawing,
/// transferring money, and viewing transactions.
/// </summary>
public interface IAccountService
{
    // Creates a new bank account with the specified details.
    Task<IBankAccount> CreateBankAccountAsync(string name, AccountType accountType, string currency, decimal initialBalance);

    // Retrieves all existing bank accounts.
    Task<List<IBankAccount>>GetAccountsAsync();

    // Deletes a specific bank account by its unique identifier.
    Task DeleteAccountAsync(Guid id);

    // Deposits the specified amount into an account.
    Task DepositAsync(Guid accountId, decimal amount, string? note = null);

    // Withdraws the specified amount from an account.
    Task WithdrawAsync(Guid accountId, decimal amount, string? note = null);

    // Transfers the specified amount between two accounts.
    Task TransferAsync(Guid fromAccountId, Guid toAccountId, decimal amount, string? note = null);

    // Retrieves all transactions, optionally filtered by account.
    Task<List<Transaction>> GetTransactionsAsync(Guid? accountId = null);

    // Exports all account and transaction data as a JSON string for backup.
    Task<string> ExportJsonAsync();

    // Imports account and transaction data from a provided Json string.
    // Can replace or merge with existing data depending on "replaceExisting".
    Task<List<string>> ImportJsonAsync(string json,bool replaceExisting = true);

    // Adds interest automatically to all savings accounts on a regular schedule. 
    void AutoInterestUpdates();

    // Triggered whenever interest has been successfully applied to one or more accounts.
    event EventHandler<InterestAppliedEventArgs>? InterestApplied;
}
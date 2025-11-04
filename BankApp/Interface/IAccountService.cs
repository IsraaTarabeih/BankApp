namespace BankApp.Interface;

/// <summary>
/// Defines the contract for account related operations.
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
}
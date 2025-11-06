namespace BankApp.Interface;

///<summary>
/// Represents the core structure and functionality of a bank account.
/// </summary>
public interface IBankAccount
{
    Guid Id { get; }
    string Name { get; }
    AccountType AccountType { get; }
    string Currency {  get; }
    decimal Balance { get; }
    DateTime LastUpdated { get; }
    DateTime? LastInterestApplied { get; }

    // Deposits the specified amount into the account.
    void Deposit (decimal amount); 

    // Withdraws the specified amount from the account.
    void Withdraw (decimal amount);
}
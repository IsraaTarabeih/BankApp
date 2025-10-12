namespace BankApp.Interface
    ///<summary>
    ///Interface containing the BankAccount methods 
    /// </summary>
{
    public interface IBankAccount
    {
        Guid Id { get; }
        string Name { get; }
        string Currency {  get; }
        decimal Balance { get; }
        DateTime LastUpdated { get; }

        void Withdraw (decimal amount); 
        void Deposit (decimal amount);

    }
}

namespace BankApp.Services

// Represents event data for when interest is applied to an account, 
// including which account was affected, the interest amount, and the date/time it was applied.
{
    public sealed class InterestAppliedEventArgs : EventArgs
    {
        public Guid AccountId { get; init; }
        public Decimal Amount { get; init; }
        public DateTime When { get; init; }
    }
}
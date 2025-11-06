namespace BankApp.Interface
{
    /// <summary>
    /// Simple UI lock using a fixed (predefined *check README*) 4-digit PIN.
    /// Keeps an "unlocked" flag in local storage. 
    /// </summary>
    public interface IPinLockService
    {
        // Returns true when the UI is unlocked.
        bool IsUnlocked { get; }

        // Load the current unlocked state from storage.
        Task InitializedAsync();

        // Checks the entered code against the predefined PIN.
        // If correct, marks the UI as unlocked and saves that state.
        Task<bool> VerifyAsync(string code);

        // Locks the UI and clears the saved unlocked state.
        Task LockAsync();
    }
}
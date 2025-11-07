namespace BankApp.Services
{
    /// <summary>
    /// Handles the PIN lock logic for the app.
    /// Uses a fixed 4-digit PIN and stores the unlock state in local storage.
    /// </summary>
    public class PinLockService : IPinLockService
    {
        // Key for saving unlock state.
        private const string UnlockedKey = "pin_unlocked";

        //Access to browser local storage.
        private readonly ILocalStorageService _localStorage;

        // The predetermined PIN code.
        private readonly string _fixedPin;
         
        // True if app is currently unlocked.
        public bool IsUnlocked { get; private set; }

        // Sets up the service with local storage and my fixed PIN.
        public PinLockService(ILocalStorageService localStorage, string fixedPin)
        {
            _localStorage = localStorage;
            _fixedPin = fixedPin;
        }

        // Loads the unlocked state from local storage when the app starts.
        public async Task InitializedAsync()
        {
            IsUnlocked = await _localStorage.GetItemAsync<bool>(UnlockedKey);
        }

        // Tries to unlock the app with the entered PIN.
        // Saves the state if the code is correct.
        public async Task<bool> VerifyAsync(string code)
        {
            if (string.Equals(code, _fixedPin, StringComparison.Ordinal))
            {
                IsUnlocked = true;
                await _localStorage.SetItemAsync(UnlockedKey, true);
                return true;
            }
            return false;
        }

        // Locks the app and removes the unlock flag from storage.
        public async Task LockAsync()
        {
            IsUnlocked = false;
            await _localStorage.RemoveItemAsync(UnlockedKey);
        }
    }
}
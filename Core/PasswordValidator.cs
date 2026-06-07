using System;

namespace BruteforceApp1.Core
{
    // Checks if a candidate password matches the target hash
    // This class knows NOTHING about how candidates are generated
    public class PasswordValidator
    {
        private readonly string _targetHash;

        public PasswordValidator(string targetHash)
        {
            if (string.IsNullOrEmpty(targetHash))
                throw new ArgumentException("Target hash cannot be empty.");
            _targetHash = targetHash;
        }

        // Called millions of times during attack
        public bool IsMatch(string candidate)
        {
            return PasswordHasher.Verify(candidate, _targetHash);
        }
    }
}
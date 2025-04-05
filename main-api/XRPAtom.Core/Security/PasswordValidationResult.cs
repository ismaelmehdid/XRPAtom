namespace XRPAtom.Core.Security
{
    /// <summary>
    /// Represents the result of a password validation process
    /// </summary>
    public class PasswordValidationResult
    {
        /// <summary>
        /// Indicates whether the password validation was successful
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// A collection of error messages if validation failed
        /// </summary>
        public IReadOnlyCollection<string> Errors { get; }

        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        private PasswordValidationResult()
        {
            Succeeded = true;
            Errors = Array.Empty<string>();
        }

        /// <summary>
        /// Creates a failed validation result with error messages
        /// </summary>
        /// <param name="errors">Error messages describing validation failures</param>
        private PasswordValidationResult(IEnumerable<string> errors)
        {
            Succeeded = false;
            Errors = errors.ToList().AsReadOnly();
        }

        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        public static PasswordValidationResult Success() => new PasswordValidationResult();

        /// <summary>
        /// Creates a failed validation result
        /// </summary>
        /// <param name="errors">Error messages describing validation failures</param>
        public static PasswordValidationResult Failed(params string[] errors) => 
            new PasswordValidationResult(errors);

        /// <summary>
        /// Combines multiple validation results
        /// </summary>
        /// <param name="results">Results to combine</param>
        public static PasswordValidationResult Combine(params PasswordValidationResult[] results)
        {
            var failedResults = results.Where(r => !r.Succeeded).ToList();
            
            return failedResults.Any() 
                ? Failed(failedResults.SelectMany(r => r.Errors).ToArray()) 
                : Success();
        }
    }
}
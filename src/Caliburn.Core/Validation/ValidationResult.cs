using System;

namespace Caliburn.Light
{
    /// <summary>
    /// Represents a container for the results of a validation request.
    /// </summary>
    public sealed class ValidationResult
    {
        private ValidationResult(bool isValid, string errorDescription)
        {
            IsValid = isValid;
            ErrorDescription = errorDescription;
        }

        /// <summary>
        /// Gets a value indicating whether validation was sucessful.
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Gets the error message for the validation.
        /// </summary>
        public string ErrorDescription { get; private set; }

        private static readonly ValidationResult ValidResult = new ValidationResult(true, null);


        /// <summary>
        /// Represents the success of the validation.
        /// </summary>
        /// <returns></returns>
        public static ValidationResult Success()
        {
            return ValidResult;
        }

        /// <summary>
        /// Represents failure of the validation.
        /// </summary>
        /// <param name="errorDescription">The error description.</param>
        /// <returns></returns>
        public static ValidationResult Failure(string errorDescription)
        {
            return new ValidationResult(false, errorDescription);
        }
    }
}

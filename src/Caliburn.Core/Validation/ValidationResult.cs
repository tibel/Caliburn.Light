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
        /// <param name="provider">An object that supplies culture-specific formatting information. </param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        public static ValidationResult Failure(IFormatProvider provider, string format, params object[] args)
        {
            var formattedMessage = string.Format(provider, format, args);
            return new ValidationResult(false, formattedMessage);
        }
    }
}

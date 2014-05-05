using System.Globalization;

namespace Caliburn.Light
{
    /// <summary>
    /// Performs a length validation of a <see cref="string"/>.
    /// </summary>
    public class StringLengthValidationRule : ValidationRule<string>
    {
        private readonly int _minimumLength;
        private readonly int _maximumLength;
        private readonly string _errorMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringLengthValidationRule"/> class.
        /// </summary>
        /// <param name="minimumLength">The minimum length.</param>
        /// <param name="maximumLength">The maximum length.</param>
        /// <param name="errorMessage">The error message.</param>
        public StringLengthValidationRule(int minimumLength, int maximumLength, string errorMessage)
        {
            _minimumLength = minimumLength;
            _maximumLength = maximumLength;
            _errorMessage = errorMessage;
        }

        /// <summary>
        /// Performs validation checks on a value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="cultureInfo">The culture to use in this rule.</param>
        /// <returns>A <see cref="ValidationResult" /> object.</returns>
        protected override ValidationResult OnValidate(string value, CultureInfo cultureInfo)
        {
            var length = 0;

            if (!string.IsNullOrEmpty(value))
                length = value.Trim().Length;

            if (length < _minimumLength || length > _maximumLength)
                return ValidationResult.Failure(cultureInfo, _errorMessage, _minimumLength, _maximumLength, length);

            return ValidationResult.Success();
        }
    }
}

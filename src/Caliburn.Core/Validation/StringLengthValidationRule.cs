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
                length = GetTrimmedLength(value);
            
            if (length < _minimumLength || length > _maximumLength)
                return ValidationResult.Failure(cultureInfo, _errorMessage, _minimumLength, _maximumLength, length);

            return ValidationResult.Success();
        }

        private static int GetTrimmedLength(string value)
        {
            //end will point to the first non-trimmed character on the right
            //start will point to the first non-trimmed character on the Left
            var end = value.Length - 1;
            var start = 0;

            for (; start < value.Length; start++)
            {
                if (!char.IsWhiteSpace(value[start])) break;
            }

            for (; end >= start; end--)
            {
                if (!char.IsWhiteSpace(value[end])) break;
            }

            return end - start + 1;
        }
    }
}
